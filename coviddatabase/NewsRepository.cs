using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using covidlibrary;
using Dapper;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace coviddatabase
{
    public class NewsRepository : IRepository<NewsEntity>
    {
        private string connectionString;
        private IConfiguration configuration;

        public NewsRepository(IConfiguration configuration)
        {
            this.configuration = configuration;
            connectionString = configuration.GetConnectionString("CovidConnection");
            Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;
        }

        private readonly CovidContext _context;

        //public NewsRepository(CovidContext context)
        //{
        //    _context = context;
        //}


        internal NpgsqlConnection Connection
        {
            get
            {
                return new NpgsqlConnection(connectionString);
            }
        }

        //internal CovidContext Context
        //{
        //    get
        //    {
        //        return new CovidContext();
        //    }
        //}


        public void Add(NewsEntity item)
        {
            using (var dbConnection = Connection)
            {
                dbConnection.Open();
                int add = dbConnection.Execute("INSERT INTO news (title,content,text_source,source,lang) VALUES(@Title,@Content,@TextSource,@Source,@Lang)", item);
            }
        }

        public int Count()
        {
            using (var dbConnection = Connection)
            {
                dbConnection.Open();
                return dbConnection.ExecuteScalar<int>("SELECT COUNT(*) FROM news");
            }
        }

        public IEnumerable<NewsEntity> FindAll()
        {
            using (var dbConnection = Connection)
            {
                dbConnection.Open();
                return dbConnection.Query<NewsEntity>("SELECT * FROM news");
            }
        }

        public NewsEntity FindByID(int id)
        {
            using (var dbConnection = Connection)
            {
                dbConnection.Open();
                return dbConnection.Query<NewsEntity>("SELECT * FROM news WHERE id = @Id", new { Id = id }).FirstOrDefault();
            }
        }

        public void Remove(int id)
        {
            using (var dbConnection = Connection)
            {
                dbConnection.Open();
                dbConnection.Execute("DELETE FROM news WHERE Id=@Id", new { Id = id });
            }
        }

        public IEnumerable<NewsEntity> Take(int nb, int skip = 0)
        {
            using (var dbConnection = Connection)
            {
                dbConnection.Open();
                return dbConnection.Query<NewsEntity>("SELECT * FROM news LIMIT @nb OFFSET @skip", new { nb = nb, skip = skip });
            }
        }

        public void Update(NewsEntity item)
        {
            using (var dbConnection = Connection)
            {
                dbConnection.Open();
                item.DateUpdate = DateTime.Now;
                dbConnection.Query("UPDATE news SET title = @Title, content = @Content, text_source = @TextSource, source = @Source, lang = @Lang, date_update = @DateUpdate WHERE id = @Id", item);
            }
        }
    }
}
