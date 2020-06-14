using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Web;
using VocaDb.Model.Domain.Web;

namespace VocaDb.Web {

	public class HttpRequest : IHttpRequest {

		public HttpRequest(HttpRequestBase request) {
			this.request = request;
		}

		private readonly HttpRequestBase request;

		public IDictionary<string, ICookie> Cookies => throw new NotImplementedException();
		public NameValueCollection Form => request.Form;
		public NameValueCollection Params => request.Params;
		public NameValueCollection QueryString => request.QueryString;
		public string UserHostAddress => request.UserHostAddress;

	}

}