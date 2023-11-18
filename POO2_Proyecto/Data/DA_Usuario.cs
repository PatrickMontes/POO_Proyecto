using POO2_Proyecto.Models;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace POO2_Proyecto.Data
{
    public class DA_Usuario
    {
        private readonly string connectionString;

        public DA_Usuario(string connectionString)
        {
            this.connectionString = connectionString;
        }

       /* public List<Usuario> ListaUsuario()
        {
            List<Usuario> usuarios = new List<Usuario>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = "SELECT IdUsuario, Nombre, Correo, Clave, IdRol FROM Usuario";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Usuario usuario = new Usuario
                            {
                                IdUsuario = (int)reader["IdUsuario"],
                                Nombre = reader["Nombre"].ToString(),
                                Correo = reader["Correo"].ToString(),
                                Clave = reader["Clave"].ToString(),
                                IdRol = (Rol)reader["IdRol"]
                            };

                            usuarios.Add(usuario);
                        }
                    }
                }
            }

            return usuarios;
        }*/

        public Usuario ValidarUsuario(string correo, string clave)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = "SELECT IdUsuario, Nombre, Correo, Clave, IdRol FROM Usuario WHERE Correo = @Correo AND Clave = @Clave";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Correo", correo);
                    command.Parameters.AddWithValue("@Clave", clave);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new Usuario
                            {
                                IdUsuario = (int)reader["IdUsuario"],
                                Nombre = reader["Nombre"].ToString(),
                                Correo = reader["Correo"].ToString(),
                                Clave = reader["Clave"].ToString(),
                                IdRol = (Rol)reader["IdRol"]
                            };
                        }
                    }
                }
            }

            return null;
        }
    }
}
