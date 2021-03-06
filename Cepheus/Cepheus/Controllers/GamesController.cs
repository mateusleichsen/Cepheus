﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using Cepheus.Entities;
using Cepheus.Infrastructure;
using Cepheus.Models;

namespace Cepheus.Controllers
{
    [TokenValidation]
    public class GamesController : ApiController
    {
        #region Private Properties

        readonly Repository<Game> _repository;
        readonly DbContext _context; 

        #endregion

        #region Constructor

        public GamesController()
        {
            this._context = new CepheusContext(false);
            this._repository = new Repository<Game>(this._context);
        }

        #endregion

        #region Actions

        [HttpGet]
        public IQueryable<Game> Get()
        {
            var result = this._repository.Get(e => e.Developer, e => e.GameAndTypes, e => e.GameAndTypes.Select(c => c.GameType));

            if (result == null || result.Count() == 0)
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.NotFound));

            return result;
        }

        [HttpGet]
        public Game Get(int id)
        {
            var result = this._repository.Get(e => e.GameId == id, e => e.Developer, e => e.GameAndTypes, e => e.GameAndTypes.Select(c => c.GameType))
                  .FirstOrDefault();

            if (result == null)
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.NotFound));

            return result;
        }

        [HttpGet]
        [ActionName("Search")]
        public IQueryable<Game> Search(string value)
        {
            var result = this._repository.Get(e => e.Name.Contains(value), e => e.GameAndTypes, e => e.Developer, e => e.GameAndTypes.Select(c => c.GameType));

            if (result == null)
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.NotFound));

            return result;
        }

        [HttpGet]
        public HttpResponseMessage Image(int id)
        {
            var game = this._repository.Get(e => e.GameId == id)
                .FirstOrDefault();
            if (game == null)
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.NotFound));

            var path = string.Format(@"C:\Temp\{0}_{1}.jpg", id, game.Name);
            var stream = new FileStream(path, FileMode.Open);

            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new StreamContent(stream);
            response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment");
            response.Content.Headers.ContentDisposition.FileName = "test.jpg";
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            response.Content.Headers.ContentLength = stream.Length;

            return response;
        }

        [HttpPost]
        public HttpResponseMessage Post(Game value)
        {
            if (value == null)
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.BadRequest));

            var typeHelper = new GameTypeHelper(new Repository<GameType>(this._context));

            if (value.GameAndTypes != null && value.GameAndTypes.Count > 0)
            {
                typeHelper.FixDuplicationGameTypes(value.GameAndTypes);
                typeHelper.FixInvalidDatas(value);
            }

            this._repository.Add(value);
            this._context.SaveChanges();

            return Request.CreateResponse<Game>(HttpStatusCode.Created, value);
        }

        [HttpPut]
        public HttpResponseMessage Put(int id, Game value)
        {
            var game = this._repository.Get(e => e.GameId == id)
                .FirstOrDefault();

            if (value == null || game == null)
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.BadRequest));

            game.Description = value.Description;
            game.DeveloperId = value.DeveloperId;
            game.Developer = null;
            game.Image = value.Image;
            game.Name = value.Name;
            game.GameAndTypes = null;
            game.GameAndTypes = value.GameAndTypes;

            var typeHelper = new GameTypeHelper(new Repository<GameType>(this._context));

            if (game.GameAndTypes != null && game.GameAndTypes.Count > 0)
            {
                typeHelper.FixDuplicationGameTypes(game.GameAndTypes);
                typeHelper.FixInvalidDatas(game);
            }

            this._repository.Update<Game>(game);
            this._context.SaveChanges();

            return Request.CreateResponse<Game>(HttpStatusCode.OK, value);
        }

        [HttpDelete]
        public HttpResponseMessage Delete(int id)
        {
            var value = this._repository.Get(e => e.GameId == id).SingleOrDefault();

            if (value == null)
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.BadRequest));

            this._repository.Remove(value);
            this._context.SaveChanges();

            return Request.CreateResponse(HttpStatusCode.OK);
        }

        #endregion

        #region Override

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            this._context.Dispose();
        }

        #endregion
    }
}
