﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Server.DTOs;
using Server.Mappers;
using Server.Models;
using Server.Services;
using Stripe.Climate;

namespace Server.Controllers;

[Route("api/[controller]")]
[ApiController]
public class OrderController : ControllerBase
{
    private readonly UserService _userService;

    public OrderController(UserService userService)
    {
        _userService = userService;
    }


    [Authorize]
    [HttpGet("allUserOrders")]
    public async Task<IEnumerable<Models.Order>> GetAllOrders()
    {
        User user = await GetCurrentUser();

        if (user == null)
        {
            return null;
        }

        return user.Orders.OrderByDescending(o => o.CreatedAt);

    }



    private async Task<User> GetCurrentUser()
    {
        // Pilla el usuario autenticado según ASP
        System.Security.Claims.ClaimsPrincipal currentUser = this.User;
        string idString = currentUser.Claims.First().ToString().Substring(3); // 3 porque en las propiedades sale "id: X", y la X sale en la tercera posición

        // Pilla el usuario de la base de datos
        return await _userService.GetUserAndOrdersFromDbByStringId(idString);
    }
}
