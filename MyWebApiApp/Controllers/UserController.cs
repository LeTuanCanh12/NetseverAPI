using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MyWebApiApp.Data;
using MyWebApiApp.Models;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Console;

namespace MyWebApiApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly MyDbContext _context;
        private readonly AppSetting _appSettings;

        public UserController(MyDbContext context, IOptionsMonitor<AppSetting> optionsMonitor)
        {
            _context = context;
            _appSettings = optionsMonitor.CurrentValue;
        }

        //Lấy thông tin người dùng chat gần nhất với Login User
        [HttpGet("GetUsers")]
        public IActionResult GetUsers(int rootUserId) 
        { 
            try
            {
                var result = (from chat in _context.Chats
                    join sender in _context.NguoiDungs on chat.SenderId equals sender.Id
                    join receiver in _context.NguoiDungs on chat.ReceiverId equals receiver.Id
                    join chatInfo in _context.ChatInfos on chat.ChatId equals chatInfo.ChatId
                    where chat.SenderId == rootUserId || chat.ReceiverId == rootUserId
                    orderby chatInfo.SendDate descending
                    group new
                    {
                        UserId = sender.Id == rootUserId ? receiver.Id : sender.Id,
                        UserName = sender.Id == rootUserId ? receiver.UserName : sender.UserName,
                        SendDate = chatInfo.SendDate
                    } by new { UserId = sender.Id == rootUserId ? receiver.Id : sender.Id } into grouped
                    select new
                    {
                        UserId = grouped.Key.UserId,
                        UserName = grouped.First().UserName
                    }).Distinct().Take(6).ToList();

                bool hasGPT = false;
                foreach (var li in result){
                    if(li.UserId == 1)
                    {
                        hasGPT = true;
                        var temp = li;
                        result.Remove(li);
                        result.Insert(0,temp);
                        break;
                    }
                }
                if (!hasGPT) { 
                    if (result.Count() == 6) 
                    { 
                        result.RemoveAt(result.Count - 1);
                    }
                    result.Insert(0,new { UserId= 1 ,UserName="GPT"});
                }


                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "Success to ge user",
                    Data = result
                });
            }
            catch
            {
                return Ok(new ApiResponse
                {
                    Success = false,
                    Message = "Fail to get user"
                });
            }
        }

        //Tìm kiếm người dùng bằng tên
        [HttpGet()]
        public IActionResult FindUserByName(string userName)
        {
            try
            {
                var users = _context.NguoiDungs.Where(user => user.UserName.Contains(userName))
                    .OrderByDescending(user => user.UserName.IndexOf(userName))
                    .Take(6).ToList();
                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "Success find user by name",
                    Data = users.ToList()
                });
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return Ok(
                    new ApiResponse
                    {
                        Success = false,
                        Message = "Fail to find user"
                    }
                );
            }
        }

        //Đăng ký tài khoản
        [HttpPost("Register")]
        public IActionResult Register(string UserName, string Password, string HoTen, string Email)
        {
            try
            {
                NguoiDung user = new NguoiDung
                {
                    UserName = UserName,
                    Password = Password,
                    HoTen = HoTen,
                    Email = Email
                };
                _context.Add(user);
                _context.SaveChanges();
                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "Success register",
                    Data = user
                });
            }
            catch
            {
                return Ok(new ApiResponse
                {
                    Success = false,
                    Message = "Fail to register"
                });
            }
        }

        //Xóa tài khoản
        [HttpDelete("{UserName}")]
        public IActionResult Delete(string UserName) 
        {
            try
            {
                var user = _context.NguoiDungs.SingleOrDefault(user=>user.UserName == UserName);
                if (user == null)
                {
                    return Ok(new ApiResponse
                    {
                        Success = false,
                        Message = "Not found user"
                    });
                }
                _context.Remove(user);
                _context.SaveChanges(true);
                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "Registered",
                    Data = user
                });
            }
            catch
            {
                return Ok(new ApiResponse
                {
                    Success = false,
                    Data = "Fail to register"
                });
            }
        }

        //Update thông tin tài khoản
        [HttpPut()]
        public IActionResult Update(string UserName, string Password, string HoTen, string Email) 
        {
            try
            {
                var user = _context.NguoiDungs.SingleOrDefault(user => user.UserName == UserName);
                if (user == null)
                {
                    return Ok(new ApiResponse
                    {
                        Success = false,
                        Message = "Not found user"
                    });
                }
                user.UserName = UserName;
                user.Password = Password;
                user.HoTen = HoTen;
                user.Email = Email;
                _context.SaveChanges();
                return Ok(new ApiResponse 
                {
                    Success = true,
                    Message = "Success update user info",
                    Data = user
                });
            }
            catch
            {
                return Ok(new ApiResponse
                {
                    Success = false,
                    Message = "Fail to update user info"
                });
            }
        }

        //Đăng nhập
        [HttpPost("Login")]
        public async Task<IActionResult> Validate(LoginModel model)
        {
            var user = _context.NguoiDungs.SingleOrDefault(p => p.UserName == model.UserName && model.Password == p.Password);
            if (user == null) //không đúng
            {
                return Ok(new ApiResponse
                {
                    Success = false,
                    Message = "Invalid username/password"
                });
            }

            //cấp token
            var token = await GenerateToken(user);

            return Ok(new ApiResponse
            {
                Success = true,
                Message = "Authenticate success",
                Data = user
            });
        }

        //Tạo Token JWT
        private async Task<TokenModel> GenerateToken(NguoiDung nguoiDung)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();

            var secretKeyBytes = Encoding.UTF8.GetBytes(_appSettings.SecretKey);

            var tokenDescription = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] {
                    new Claim(ClaimTypes.Name, nguoiDung.HoTen),
                    new Claim(JwtRegisteredClaimNames.Email, nguoiDung.Email),
                    new Claim(JwtRegisteredClaimNames.Sub, nguoiDung.Email),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim("UserName", nguoiDung.UserName),
                    new Claim("Id", nguoiDung.Id.ToString()),

                    //roles
                }),
                Expires = DateTime.UtcNow.AddSeconds(20),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(secretKeyBytes), SecurityAlgorithms.HmacSha512Signature)
            };

            var token = jwtTokenHandler.CreateToken(tokenDescription);
            var accessToken = jwtTokenHandler.WriteToken(token);
            var refreshToken = GenerateRefreshToken();

            //Lưu database
            var refreshTokenEntity = new RefreshToken
            {
                Id = Guid.NewGuid(),
                JwtId = token.Id,
                UserId = nguoiDung.Id,
                Token = refreshToken,
                IsUsed = false,
                IsRevoked = false,
                IssuedAt = DateTime.UtcNow,
                ExpiredAt = DateTime.UtcNow.AddHours(1)
            };

            await _context.AddAsync(refreshTokenEntity);
            await _context.SaveChangesAsync();

            return new TokenModel
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };
        }

        //Refresh Token
        private string GenerateRefreshToken()
        {
            var random = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(random);

                return Convert.ToBase64String(random);
            }
        }

        //Renew Token
        [HttpPost("RenewToken")]
        public async Task<IActionResult> RenewToken(TokenModel model)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();
            var secretKeyBytes = Encoding.UTF8.GetBytes(_appSettings.SecretKey);
            var tokenValidateParam = new TokenValidationParameters
            {
                //tự cấp token
                ValidateIssuer = false,
                ValidateAudience = false,

                //ký vào token
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(secretKeyBytes),

                ClockSkew = TimeSpan.Zero,

                ValidateLifetime = false //ko kiểm tra token hết hạn
            };
            try
            {
                //check 1: AccessToken valid format
                var tokenInVerification = jwtTokenHandler.ValidateToken(model.AccessToken, tokenValidateParam, out var validatedToken);

                //check 2: Check alg
                if (validatedToken is JwtSecurityToken jwtSecurityToken)
                {
                    var result = jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha512, StringComparison.InvariantCultureIgnoreCase);
                    if (!result)//false
                    {
                        return Ok(new ApiResponse
                        {
                            Success = false,
                            Message = "Invalid token"
                        });
                    }
                }

                //check 3: Check accessToken expire?
                var utcExpireDate = long.Parse(tokenInVerification.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Exp).Value);

                var expireDate = ConvertUnixTimeToDateTime(utcExpireDate);
                if (expireDate > DateTime.UtcNow)
                {
                    return Ok(new ApiResponse
                    {
                        Success = false,
                        Message = "Access token has not yet expired"
                    });
                }

                //check 4: Check refreshtoken exist in DB
                var storedToken = _context.RefreshTokens.FirstOrDefault(x => x.Token == model.RefreshToken);
                if(storedToken == null)
                {
                    return Ok(new ApiResponse
                    {
                        Success = false,
                        Message = "Refresh token does not exist"
                    });
                }

                //check 5: check refreshToken is used/revoked?
                if (storedToken.IsUsed)
                {
                    return Ok(new ApiResponse
                    {
                        Success = false,
                        Message = "Refresh token has been used"
                    });
                }
                if (storedToken.IsRevoked)
                {
                    return Ok(new ApiResponse
                    {
                        Success = false,
                        Message = "Refresh token has been revoked"
                    });
                }

                //check 6: AccessToken id == JwtId in RefreshToken
                var jti = tokenInVerification.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Jti).Value;
                if(storedToken.JwtId != jti)
                {
                    return Ok(new ApiResponse
                    {
                        Success = false,
                        Message = "Token doesn't match"
                    });
                }

                //Update token is used
                storedToken.IsRevoked = true;
                storedToken.IsUsed = true;
                _context.Update(storedToken);
                await _context.SaveChangesAsync();

                //create new token
                var user = await _context.NguoiDungs.SingleOrDefaultAsync(nd => nd.Id == storedToken.UserId);
                var token = await GenerateToken(user);

                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "Renew token success",
                    Data = token
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = "Something went wrong"
                });
            }
        }

        private DateTime ConvertUnixTimeToDateTime(long utcExpireDate)
        {
            var dateTimeInterval = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTimeInterval.AddSeconds(utcExpireDate).ToUniversalTime();

            return dateTimeInterval;
        }
    }
}
