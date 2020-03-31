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
    public class OwnersController : ControllerBase
    {
        private readonly IConfiguration _config;

        public OwnersController(IConfiguration config)
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
        public async Task<IActionResult> Get([FromQuery] int? neighborhoodId,
            [FromQuery] string include, [FromQuery] string q)
        {


            if (include != "neighborhoodName")
            {
                var owners = GetAllOwners(neighborhoodId, q);
                return Ok(owners);
            }
            else
            {
                var owners = GetOwnerWithNeighborhood(neighborhoodId, q);
                return Ok(owners);
            };
        }
        //When the include is not equal to neighborhoodName then get all owners


        [HttpGet("{id}", Name = "GetOwner")]
        public async Task<IActionResult> Get([FromRoute] int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT
                            Id, Name, Address, Phone, NeighborhoodId
                        FROM OWNER
                        WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    Owner owner = null;

                    if (reader.Read())
                    {
                        owner = new Owner
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Name = reader.GetString(reader.GetOrdinal("Name")),
                            NeighborhoodId = reader.GetInt32(reader.GetOrdinal("Id")),
                            Address = reader.GetString(reader.GetOrdinal("Address")),
                            Phone = reader.GetString(reader.GetOrdinal("Phone")),


                        };
                    }
                    reader.Close();

                    return Ok(owner);
                }
            }
        }
        private List<Owner> GetAllOwners([FromQuery] int? neighborhoodId, [FromQuery] string q)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                    SELECT o.Id, o.Name, o.Address, o.NeighborhoodId, o.Address, o.Phone
                    FROM Owner o
                    WHERE 1=1";
                    //If the url doesn't have /neighborhoodId then return the NeighborhoodId of the Owner
                    if (neighborhoodId != null)
                    {
                        cmd.CommandText += " AND NeighborhoodId = @neighborhoodId";
                        cmd.Parameters.Add(new SqlParameter("@neighborhoodId", neighborhoodId));
                    }
                    //This is for the owner name-it will return anything that matches that you enter for Q

                    if (q != null)
                    {
                        cmd.CommandText += " AND Name LIKE @Name";
                        cmd.Parameters.Add(new SqlParameter("@Name", "%" + q + "%"));
                    }

                    SqlDataReader reader = cmd.ExecuteReader();

                    List<Owner> owners = new List<Owner>();

                    while (reader.Read())
                    {
                        Owner owner = new Owner
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Name = reader.GetString(reader.GetOrdinal("Name")),
                            Address = reader.GetString(reader.GetOrdinal("Address")),
                            Phone = reader.GetString(reader.GetOrdinal("Phone")),
                            NeighborhoodId = reader.GetInt32(reader.GetOrdinal("NeighborhoodId")),


                        };

                        owners.Add(owner);
                    }
                    reader.Close();

                    return owners;
                }
            }

        }
        //this is what will happen if the include statement does equal a neighborhood Name
        private List<Owner> GetOwnerWithNeighborhood([FromQuery] int? neighborhoodId, [FromQuery] string q)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                ///This will returnt the owner with the Neighborhood Name
                {
                    cmd.CommandText = @"
                    SELECT o.Id, o.Name, o.Address, o.NeighborhoodId, o.Address, o.Phone, n.Name AS NeighborhoodName
                    FROM Owner o
                    LEFT JOIN Neighborhood n
                    ON n.Id= o.NeighborhoodId
                    WHERE 1=1";

                    if (neighborhoodId != null)
                    {
                        cmd.CommandText += " AND NeighborhoodId = @neighborhoodId";
                        cmd.Parameters.Add(new SqlParameter("@neighborhoodId", neighborhoodId));
                    }

                    if (q != null)
                    {
                        cmd.CommandText += " AND o.Name LIKE @Name";
                        cmd.Parameters.Add(new SqlParameter("@Name", "%" + q + "%"));
                    }
                    SqlDataReader reader = cmd.ExecuteReader();

                    List<Owner> owners = new List<Owner>();

                    while (reader.Read())
                    //returns the owner with the neighborhood Name and ID
                    {
                        Owner owner = new Owner
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Name = reader.GetString(reader.GetOrdinal("Name")),
                            Address = reader.GetString(reader.GetOrdinal("Address")),
                            Phone = reader.GetString(reader.GetOrdinal("Phone")),
                            NeighborhoodId = reader.GetInt32(reader.GetOrdinal("NeighborhoodId")),
                            Neighborhood = new Neighborhood
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("NeighborhoodId")),
                                Name = reader.GetString(reader.GetOrdinal("NeighborhoodName"))
                            }

                        };

                        owners.Add(owner);
                    }
                    reader.Close();

                    return owners;
                }
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Owner owner)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"INSERT INTO Owner (Name, NeighborhoodId, Address, Phone)
                                        OUTPUT INSERTED.Id
                                        VALUES (@name, @neighborhoodId, @address, @phone)";
                    cmd.Parameters.Add(new SqlParameter("@name", owner.Name));
                    cmd.Parameters.Add(new SqlParameter("@neighborhoodId", owner.NeighborhoodId));
                    cmd.Parameters.Add(new SqlParameter("@address", owner.Address));
                    cmd.Parameters.Add(new SqlParameter("@phone", owner.Phone));


                    int newId = (int)cmd.ExecuteScalar();
                    owner.Id = newId;
                    return CreatedAtRoute("GetOwner", new { id = newId }, owner);
                }
            }
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> Put([FromRoute] int id, [FromBody] Owner owner)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"UPDATE Owner
                                            SET Name = @name,
                                                Address = @address,
                                                Phone = @phone,
                                                NeighborhoodId = @neighborhoodId
                                               
                                            WHERE Id = @id";
                        cmd.Parameters.Add(new SqlParameter("@name", owner.Name));
                        cmd.Parameters.Add(new SqlParameter("@neighborhoodId", owner.NeighborhoodId));
                        cmd.Parameters.Add(new SqlParameter("@address", owner.Address));
                        cmd.Parameters.Add(new SqlParameter("@phone", owner.Phone));

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
                if (!OwnerExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }



        private bool OwnerExists(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT Id, Name, Address, Phone, NeighborhoodId
                        FROM Owner
                        WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    SqlDataReader reader = cmd.ExecuteReader();
                    return reader.Read();
                }
            }
        }
    }
}