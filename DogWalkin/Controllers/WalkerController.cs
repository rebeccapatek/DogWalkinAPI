using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DogWalkin.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace DogWalkin.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WalkersController : ControllerBase
    {
        private readonly IConfiguration _config;

        public WalkersController(IConfiguration config)
        {
            _config = config;
        }

        public SqlConnection Connection
        {
            get
            {
                return new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            }
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT Id, Name, NeighborhoodId  FROM Walker";
                    SqlDataReader reader = cmd.ExecuteReader();
                    List<Walker> walkers = new List<Walker>();

                    while (reader.Read())
                    {
                        Walker walker = new Walker
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Name = reader.GetString(reader.GetOrdinal("Name")),
                            NeighborhoodId = reader.GetInt32(reader.GetOrdinal("NeighborhoodId")),


                        };

                        walkers.Add(walker);
                    }
                    reader.Close();

                    return Ok(walkers);
                }
            }
        }
        [HttpGet("{id}", Name = "GetWalker")]
        public async Task<IActionResult> Get([FromRoute] int id, [FromQuery] string include)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                    SELECT w.Id, w.Name, w.NeighborhoodId, n.Name AS NeighborhoodName, d.Name AS DogName";

                    if (include == "walks")
                    {
                        cmd.CommandText += ", wk.Id AS walkId, wk.Date, wk.Duration, wk.walkerId, wk.DogId ";
                    }
                    cmd.CommandText += " FROM Walker w LEFT JOIN Neighborhood n ON n.Id = w.NeighborhoodId ";
                    if (include == "walks")
                    {
                        cmd.CommandText += " LEFT JOIN Walks wk ON wk.walkerId = w.Id LEFT JOIN Dog d ON d.Id = wk.DogId";
                    }
                    cmd.CommandText += " WHERE w.Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    Walker walker = null;

                    while (reader.Read())
                    {
                        if (walker == null)
                        {

                            walker = new Walker
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                Name = reader.GetString(reader.GetOrdinal("Name")),
                                NeighborhoodId = reader.GetInt32(reader.GetOrdinal("NeighborhoodId")),
                                Neighborhood = new Neighborhood
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("NeighborhoodId")),
                                    Name = reader.GetString(reader.GetOrdinal("NeighborhoodName"))
                                },
                                Walks = new List<Walk>()

                            };
                        }

                        if (include == "walks")
                        {
                            walker.Walks.Add(new Walk()
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("WalkId")),
                                Date = reader.GetDateTime(reader.GetOrdinal("Date")),
                                Duration = reader.GetInt32(reader.GetOrdinal("Duration")),
                                DogId = reader.GetInt32(reader.GetOrdinal("DogId")),
                                Dog = new Dog
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("DogId")),
                                    Name = reader.GetString(reader.GetOrdinal("DogName"))
                                }
                            });
                        }
                    }
                    reader.Close();

                    return Ok(walker);
                }
            }
        }
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Walker walker)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"INSERT INTO Walker (Name, NeighborhoodId)
                                        OUTPUT INSERTED.Id
                                        VALUES (@name, @neighborhoodId)";
                    cmd.Parameters.Add(new SqlParameter("@name", walker.Name));
                    cmd.Parameters.Add(new SqlParameter("@neighborhoodId", walker.NeighborhoodId));


                    int newId = (int)cmd.ExecuteScalar();
                    walker.Id = newId;
                    return CreatedAtRoute("GetWalker", new { id = newId }, walker);
                }
            }
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> Put([FromRoute] int id, [FromBody] Walker walker)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"UPDATE Walker
                                            SET Name = @name,
                                                NeighborhoodId = @neighborhoodId
                                               
                                            WHERE Id = @id";
                        cmd.Parameters.Add(new SqlParameter("@name", walker.Name));
                        cmd.Parameters.Add(new SqlParameter("@neighborhoodId", walker.NeighborhoodId));


                        cmd.Parameters.Add(new SqlParameter("@id", id));

                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            return new StatusCodeResult(StatusCodes.Status204NoContent);
                        }
                        throw new Exception("No rows affected");
                    }
                }
            }
            catch (Exception)
            {
                if (!WalkerExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"DELETE FROM Walker WHERE Id = @id";
                        cmd.Parameters.Add(new SqlParameter("@id", id));

                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            return new StatusCodeResult(StatusCodes.Status204NoContent);
                        }
                        throw new Exception("No rows affected");
                    }
                }
            }
            catch (Exception)
            {
                if (!WalkerExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }
        private bool WalkerExists(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT Id, Name, NeighborhoodId
                        FROM Walker
                        WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    SqlDataReader reader = cmd.ExecuteReader();
                    return reader.Read();
                }
            }
        }
    }
}
