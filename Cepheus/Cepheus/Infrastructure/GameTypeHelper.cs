﻿using Cepheus.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Cepheus.Infrastructure
{
    public class GameTypeHelper
    {
        #region Private Properties

        private IEnumerable<GameType> _currentGameType;

        #endregion

        #region Constructor

        public GameTypeHelper(Repository<GameType> repository)
        {
            this._currentGameType = repository.Get();
        }

        #endregion

        #region Public Methods

        public void FixDuplicationGameTypes(ICollection<GameAndType> gameTypes)
        {
            foreach (var item in gameTypes)
                this.FixDuplicationGameType(item);
        }

        public void FixDuplicationGameType(GameAndType item)
        {
            if (item.GameType != null && this.HasGameType(item.GameType))
            {
                item.GameTypeId = this._currentGameType
                    .Where(c => c.Description == item.GameType.Description)
                    .First()
                    .GameTypeId;

                item.GameType = null;
            }
        }

        public void FixInvalidDatas(Game game)
        {
            var invalidDatas = new List<GameAndType>();

            foreach (var item in game.GameAndTypes)
            {
                if (!IsValidData(item))
                    invalidDatas.Add(item);

                if (item.GameTypeId != 0 &&
                    item.GameType != null &&
                    item.GameType.Description != this._currentGameType.FirstOrDefault(c => c.GameTypeId == item.GameTypeId).Description)
                    item.GameType = null;
            }

            invalidDatas.ForEach(i => game.GameAndTypes.Remove(i));
        }

        public bool IsValidData(GameAndType item)
        {
            if (item.GameTypeId != 0)
                return true;

            if (item.GameTypeId == 0 && item.GameType != null && this.IsValidGameType(item.GameType))
                return true;

            return false;
        }

        public bool HasGameType(GameType item)
        {
            return this._currentGameType
                .Select(e => e.Description)
                .Contains(item.Description);
        }

        public bool IsValidGameType(GameType item)
        {
            return !string.IsNullOrWhiteSpace(item.Description);
        }

        #endregion
    }
}