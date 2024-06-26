﻿using Business.Abstract;
using Business.Constants.Messages;
using Core.Entity.Concretes;
using Core.Utilities.Results;
using Core.Utilities.Security.Hashing;
using Core.Utilities.Security.Jwt;
using Models.Concrete;
using Models.Dtos.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Concrete
{
    
    

        public class AuthManager : IAuthService
        {
            private IUserService _userService;
            private ITokenHelper _tokenHelper;

            public AuthManager(IUserService userService, ITokenHelper tokenHelper)
            {
                _userService = userService;
                _tokenHelper = tokenHelper;
            }

            public IDataResult<User> Register(UserForRegisterDto userForRegisterDto, string password)
            {
                byte[] passwordHash, passwordSalt;
                HashingHelper.CreatePasswordHash(password, out passwordHash, out passwordSalt);
                var user = new User
                {
                    Email = userForRegisterDto.Email,
                    FirstName = userForRegisterDto.FirstName,
                    MiddleName = userForRegisterDto.MiddleName,
                    LastName = userForRegisterDto.LastName,
                    PasswordHash = passwordHash,
                    PasswordSalt = passwordSalt,
                    IdentityNumber = userForRegisterDto.IdentityNumber,
                    Phone = userForRegisterDto.Phone,
                };
                _userService.Add(user);
                return new SuccessDataResult<User>(user, Messages.UserRegistered);
            }

            public IDataResult<User> Login(UserForLoginDto userForLoginDto)
            {
                var userToCheck = _userService.GetByMail(userForLoginDto.Email);
                if (userToCheck.Data == null)
                {
                    return new ErrorDataResult<User>(userToCheck.Message);
                }

                if (!HashingHelper.VerifyPasswordHash(userForLoginDto.Password, userToCheck.Data.PasswordHash, userToCheck.Data.PasswordSalt))
                {
                    return new ErrorDataResult<User>(Messages.PasswordError);
                }

                return new SuccessDataResult<User>(userToCheck.Data, Messages.SuccessfulLogin);
            }

            public IResult UserExists(string email)
            {
            var result = _userService.GetByMail(email);
            
                if (result.Data is not null)
                {
                    return new ErrorResult(Messages.UserAlreadyExists);
                }
                return new SuccessResult();
            }

            public IDataResult<AccessToken> CreateAccessToken(User user)
            {
                var claims = _userService.GetClaims(user);
                var accessToken = _tokenHelper.CreateToken(user, claims.Data);
                return new SuccessDataResult<AccessToken>(accessToken, Messages.AccessTokenCreated);
            }

        public IResult CheckUserAuthentication(AccessToken accessToken)
        {
            var result = _tokenHelper.CheckTokenExpiration(accessToken);

            if (result.Success)
            {
                return new SuccessResult();
            }
            return new ErrorResult();
        }
    }
    }
