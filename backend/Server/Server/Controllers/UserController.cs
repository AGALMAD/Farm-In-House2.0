﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Server.DTOs;
using Server.Mappers;
using Server.Models;
using Server.Repositories;
using Server.Repositories.Base;
using Server.Services;

namespace Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly UserService _userService;

        public UserController(UserService userService)
        {
            _userService = userService;
        }

        //Obtiene todos los usuarios sin contraseña
        [Authorize]
        [HttpGet]
        public async Task<IEnumerable<UserAfterLoginDto>> GetAllUsers()
        {
            User user = await GetCurrentUser();
            
            if(user == null || !user.Role.Equals("Admin"))
            {
                return null;
            }

            //Obtiene todos los usuarios excepto el propio usuario
            IEnumerable<User> users = await _userService.GetAllUsersExceptId(user.Id);

            //Paso a DTO
            IEnumerable<UserAfterLoginDto> userDtos = _userService.ToDto(users);

            return userDtos;
        }

        [Authorize]
        [HttpGet("authorizedUser")]
        public async Task<UserAfterLoginDto> GetAuthorizedUser()
        {
            User user = await GetCurrentUser();
            if(user == null)
            {
                return null;
            }

            UserAfterLoginDto userDto = _userService.ToDto(user);

            return userDto;
        }

        [Authorize]
        [HttpPost("getJwtAfterlogin")]
        public async Task<string> GetJwt()
        {
            User user = await GetCurrentUser();
            if(user == null)
            {
                return null;
            }

            string jwt = _userService.ObtainToken(user);
            return jwt;
        }

        [Authorize]
        [HttpPut]
        public async Task UpdateUser([FromBody] User updatedUser)
        {
            User user = await GetCurrentUser();
            if (user == null)
            {
                return;
            }

            // Si intenta modificar un usuario que no es él mismo y tampoco es admin
            if (!user.Role.Equals("Admin") && updatedUser.Id != user.Id)
            {
                return;
            }

            string role = user.Role;

            if(user.Id != updatedUser.Id)
            {
                user = await _userService.GetUserByIdAsync(updatedUser.Id);
                if(user == null)
                {
                    return;
                }
            }

            user.Email = updatedUser.Email;
            user.Address = updatedUser.Address;
            user.Name = updatedUser.Name;

            if(role.Equals("Admin") && (updatedUser.Role.Equals("Admin") || updatedUser.Role.Equals("User")))
            {
                user.Role = updatedUser.Role;
            }

            if(updatedUser.Password != null && updatedUser.Password != "")
            {
                PasswordService passwordService = new PasswordService();
                user.Password = passwordService.Hash(updatedUser.Password);
            }

            await _userService.UpdateUser(user);
        }

        /*[Authorize]
        [HttpPut ("user")]
        public async Task<UserAfterLoginDto> UpdateUser([FromBody] User updatedUser)
        {
            User user = await GetCurrentUser();
            if (user == null)
            {
                return null;
            }

            User oldUser = await _userService.GetUserById(updatedUser.Id);
            oldUser.Email = updatedUser.Email;
            oldUser.Address = updatedUser.Address;
            oldUser.Name = updatedUser.Name;

            if (updatedUser.Password != null && updatedUser.Password != "")
            {
                oldUser.Password = _passwordService.Hash(updatedUser.Password);
            }

            User afterUpdate = await _userService.UpdateUser(oldUser);

            UserAfterLoginDto userDto = _userMapper.ToDto(afterUpdate);
            return userDto;
        }*/

        [Authorize]
        [HttpDelete]
        public async Task DeleteUser([FromQuery] int id)
        {
            User user = await GetCurrentUser();
            if (user == null || !user.Role.Equals("Admin") || user.Id == id)
            {
                return;
            }

            User deletedUser = await _userService.GetUserById(id);

            if (deletedUser == null)
            {
                return;
            }

            await _userService.DeleteUser(deletedUser);
        }

        /*[HttpGet("byemail")]
        public async Task<UserAfterLoginDto> GetUserByEmail(string email)
        {
            User user = await _unitOfWork.UserRepository.GetByEmailAsync(email);


            //Paso a DTO
            UserAfterLoginDto userDto = _userMapper.ToDto(user);

            return userDto;
        }*/

        private async Task<User> GetCurrentUser()
        {
            // Pilla el usuario autenticado según ASP
            System.Security.Claims.ClaimsPrincipal currentUser = this.User;
            string idString = currentUser.Claims.First().ToString().Substring(3); // 3 porque en las propiedades sale "id: X", y la X sale en la tercera posición

            // Pilla el usuario de la base de datos
            return await _userService.GetUserFromDbByStringId(idString);
        }

    }
}
