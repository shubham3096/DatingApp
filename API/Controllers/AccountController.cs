using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace API.Controllers
{
    public class AccountController : BaseApiController
    {
        private readonly DataContext _context;
        private readonly ITokenService _tokenService;

        public AccountController(DataContext context,ITokenService tokenService)
        {
            _context = context;
            _tokenService = tokenService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
        {
            if (await UserExists(registerDto.Username)) return BadRequest("Username is taken");    // to check username exists or not

            using var hmac = new HMACSHA512();    // for converting password into hashing
            var user = new AppUser
            {
                UserName = registerDto.Username.ToLower(),     //set all username to lowercae
                PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password)),
                PasswordSalt = hmac.Key
            };
            _context.Users.Add(user);      // begin tracking ==> insertion
            await _context.SaveChangesAsync();
            return new UserDto
            {
                Username =  user.UserName,
                Token =  _tokenService.CreateToken(user)
            };
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
        {
            var user = await  _context.Users.
                SingleOrDefaultAsync(x=> x.UserName == loginDto.Username);
            //Find () use with primary property
            //FirstOrDefaultAsync()   = first or default value but not retrun any exception if same sequence found
            // SingleOrDefaultAsync()  = return only element with exception
            if (user == null) return Unauthorized("Invalid username");
            using var hmac =  new HMACSHA512(user.PasswordSalt);
            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));
            for (int i = 0; i < computedHash.Length; i++)
            {
                if (computedHash[i] != user.PasswordHash[i]) return Unauthorized("Invalid Password");
            }
            return new UserDto
            {
                Username = user.UserName,
                Token = _tokenService.CreateToken(user)
            };
        }

        // helper Functions


        /// <summary>
        /// To check eisting user in db
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        private async Task<bool> UserExists(string username)
        {
            return await _context.Users.AnyAsync(x=> x.UserName == username.ToLower());
        } 
    }
}
