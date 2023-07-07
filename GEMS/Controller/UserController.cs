using GEMS.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace GEMS.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly GemsContext _context;
        public UserController(GemsContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpGet]
        [Route("GetUsers")]
        public IList<TblUser> GetUsers()
        {
            return _context.TblUsers.OrderBy(x => x.FirstName).ToList();
        }
        [HttpGet]
        [Route("GetUser/{id}")]
        public TblUser GetUser(int id)
        {
            return _context.TblUsers.Where(x => x.UserId == id).FirstOrDefault();
        }

        [HttpPost]
        [Route("SaveUser")]
        public string SaveUser([FromBody] TblUser user)
        {
            if (user != null)
            {
                if (user.UserId == 0)
                {
                    _context.TblUsers.Add(user);
                }
                else
                {
                    _context.TblUsers.Update(user);
                }
                _context.SaveChanges();
                return "User Details Saved";
            }
            else
            {
                return null;
            }

        }
        [HttpGet]
        [Route("DeleteUser/{id}")]
        public string DeleteUser(int id)
        {
            var user = _context.TblUsers.Where(x => x.UserId == id).SingleOrDefault();
            if (user != null)
            {
                _context.Remove(user);
                _context.SaveChanges();
            }
            return "User Details Deleted";
        }

        [HttpPost]
        [Route("Login")]
        [AllowAnonymous]
        public string Login([FromBody] Login login)
        {
            var stringToken = string.Empty;

            //    var parms = new List<SqlParameter>    {
            //        new SqlParameter("Username", login.Username),new SqlParameter("Password", login.Password)};
            //    var user = _context.TblUsers
            //.FromSqlRaw<TblUser>($"EXECUTE [dbo].[SP_Login] @Username,@Password", parms.ToArray())
            //.ToList();
            //var dt = new DataTable();
            //using (SqlConnection con = new SqlConnection(_configuration["ConnectionStrings"]))
            //{
            //    var command = new SqlCommand("SP_Login", con);
            //    command.CommandType = CommandType.StoredProcedure;
            //    command.Parameters.Add(new SqlParameter("Username", login.Username));
            //    command.Parameters.Add(new SqlParameter("Password", login.Password));
            //    dt = new DataTable();
            //    SqlDataAdapter da = new SqlDataAdapter(command);
            //    da.Fill(dt);
            //}
            var user = _context.TblUsers.Where(x => x.Username.ToLower() == login.Username.Trim().ToLower() && x.Password.ToLower() == login.Password.Trim().ToLower()).FirstOrDefault();
            if (user != null)
            {
                var issuer = _configuration["Jwt:Issuer"];
                var audience = _configuration["Jwt:Audience"];
                var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new[]
                    {
                new Claim("UserId",user.UserId.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti,
                Guid.NewGuid().ToString())
             }),
                    Expires = DateTime.UtcNow.AddMinutes(300),
                    Issuer = issuer,
                    Audience = audience,
                    SigningCredentials = new SigningCredentials
                    (new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha512Signature)
                };
                var tokenHandler = new JwtSecurityTokenHandler();
                var token = tokenHandler.CreateToken(tokenDescriptor);
                var jwtToken = tokenHandler.WriteToken(token);
                stringToken = tokenHandler.WriteToken(token);
            }
            else
            {
                return null;
            }
            return stringToken;
        }
    }

    public class Login
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
