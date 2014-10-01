using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;
using CsvHelper;
using CsvHelper.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace Minimod.CsvHelperMvcActionResult
{
    /// <summary>
    /// <h1>Minimod.CsvHelperMvcActionResult, Version 0.9.0, Copyright © Lars Corneliussen 2014</h1>
    /// <para>ActionResult for generating downloadable csv-files using CsvHelper</para>
    /// </summary>
    /// <remarks>
    /// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License.
    /// http://www.apache.org/licenses/LICENSE-2.0
    /// </remarks>
    internal class CsvHelperFileResult<TRecord> : FileResult
    {
        public IEnumerable<TRecord> Records { get; set; }
        public CsvConfiguration Configuration { get; private set; }

        private Encoding _encoding = Encoding.GetEncoding(1252);

        public Encoding Encoding
        {
            get { return _encoding; }
            set { _encoding = value; }
        }

        public CsvHelperFileResult(IEnumerable<TRecord> records, string delimiter)
            : this(records, csv => csv.Delimiter = delimiter)
        {

        }

        public CsvHelperFileResult(IEnumerable<TRecord> records = null, Action<CsvConfiguration> configure = null, CsvConfiguration configuration = null)
            : base("text/csv")
        {
            Records = records;
            Configuration = configuration ?? new CsvConfiguration();
            if (configure != null)
            {
                configure(Configuration);
            }
            else
            {
                throw new Exception("asdfasdfasdf");
            }
        }

        protected override void WriteFile(HttpResponseBase response)
        {
            response.ContentEncoding = Encoding;
            using (var csv = new CsvWriter(response.Output, Configuration))
            {
                csv.WriteRecords(Records);
            }
        }
    }

    internal class CsvHelperFileResult<TRecord, TRecordMap> : CsvHelperFileResult<TRecord>
        where TRecordMap : CsvClassMap<TRecord>
    {
        public CsvHelperFileResult(IEnumerable<TRecord> records = null, Action<CsvConfiguration> configure = null, CsvConfiguration configuration = null)
            : base(records, configure, configuration)
        {
            Configuration.RegisterClassMap<TRecordMap>();
        }

        public CsvHelperFileResult(IEnumerable<TRecord> records, string delimiter)
            : base(records, delimiter)
        {
            Configuration.RegisterClassMap<TRecordMap>();
        }
    }

}