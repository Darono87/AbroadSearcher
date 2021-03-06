using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly DataContext context;
        private readonly ITokenService tokenService;

        public AccountController(DataContext context, ITokenService tokenService)
        {
            this.context = context;
            this.tokenService = tokenService;
        }
        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<ActionResult<AppUser>> Register(RegisterDTO dto){
            if(await isUserInDatabase(dto.Username))
                return BadRequest("Username has been taken.");

            using var hmac = new HMACSHA512();
            AppUser newlyCreated = new AppUser{
                UserName=dto.Username.ToLower(),
                PasswordHash=hmac.ComputeHash(Encoding.UTF8.GetBytes(dto.Password)),
                PasswordSalt=hmac.Key
            };
            context.Users.Add(newlyCreated);
            await context.SaveChangesAsync();

            return newlyCreated;
        }
        
        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<ActionResult<UserDTO>> Login(LoginDTO dto){
            var user = await context.Users.SingleOrDefaultAsync(user => user.UserName == dto.Username);
            if(user == null)
                return Unauthorized("Invalid Username");
            using var hmac = new HMACSHA512(user.PasswordSalt);
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(dto.Password));
            for(int i = 0; i < hash.Length; i++){
                if(hash[i] != user.PasswordHash[i]) return Unauthorized("Wrong Password");
            }
            return new UserDTO{
                Username = user.UserName,
                Token = this.tokenService.CreateToken(user)
            };
        }

        private async Task<bool> isUserInDatabase(string username){
            return await context.Users.AnyAsync(user=>user.UserName == username.ToLower());
        }

    }
}