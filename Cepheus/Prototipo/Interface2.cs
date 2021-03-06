﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Cepheus.DataAccess;
using Cepheus.DataAccess.Enums;

namespace Prototipo
{
    public partial class Interface2 : Form
    {
        private WebApiRequester requester = new WebApiRequester(eMediaType.Json);

        public Interface2()
        {
            InitializeComponent();
            cbxMethod.Items.Add(HttpMethod.Get);
            cbxMethod.Items.Add(HttpMethod.Post);
            cbxMethod.Items.Add(HttpMethod.Put);
            cbxMethod.Items.Add(HttpMethod.Delete);
            cbxMethod.SelectedIndex = 0;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var url = txtTraceUrl.Text;
            var method = cbxMethod.SelectedItem.ToString().ToHttpMethod();
            if (method != HttpMethod.Get)
            {
                MessageBox.Show("Não implementado");
                return;
            }

            var result = requester.RequestResource(url, method);
            ShowResponse(result);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void ShowResponse(HttpResponseMessage response)
        {
            var text = "Http Version: " + response.Version + "" + response.StatusCode.ToString();
            text += Environment.NewLine + "Content-Type: " + response.Content.Headers.ContentType.MediaType + "; " + response.Content.Headers.ContentType.CharSet;
            text += Environment.NewLine + "Server: " + response.Headers.Server.First().Product.Name + " - " + response.Headers.Server.First().Product.Version;
            text += Environment.NewLine + "Content-Lenght: " + response.Content.Headers.ContentLength;
            text += Environment.NewLine + Environment.NewLine + "Body:" + Environment.NewLine + response.Content.ReadAsStringAsync().Result;

            textBox2.Text = text;
        }
    }

    public static class StringExtensions
    {
        public static HttpMethod ToHttpMethod(this string value)
        {
            if (value.Equals(HttpMethod.Get.ToString()))
                return HttpMethod.Get;

            if (value.Equals(HttpMethod.Post.ToString()))
                return HttpMethod.Post;

            if (value.Equals(HttpMethod.Put.ToString()))
                return HttpMethod.Put;

            if (value.Equals(HttpMethod.Delete.ToString()))
                return HttpMethod.Delete;

            throw new NotSupportedException("HttpMethod not supported by the Api");
        }
    }
}
