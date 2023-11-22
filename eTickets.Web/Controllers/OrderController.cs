﻿using eTickets.Data.Services.IRepositories;
using eTickets.Data.Services.Repositories;
using eTickets.Models;
using eTickets.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace eTickets.Web.Controllers
{
    public class OrderController : Controller
    {
        private readonly IMovieRepository _movieRepository;
        private readonly IOrderService _orderService;
        private readonly ShoppingCart _shoppingCart;

        public OrderController(IMovieRepository movieRepository, ShoppingCart shoppingCart, IOrderService orderService)
        {
            _movieRepository = movieRepository;
            _shoppingCart = shoppingCart;
            _orderService = orderService;
        }

        public  IActionResult ShoppingCart()
        {
            List<ShoppingCartItem> shoppingCartItemsList =  _shoppingCart.GetShoppingCartItems();

            double cartTotalPrice =  _shoppingCart.GetShoppingCartTotal();

            ShoppingCartVM shoppingCartVM = new() 
            {
                ShoppingCartItems = shoppingCartItemsList,
                ShoppingCartTotal = cartTotalPrice
            };

            return View(shoppingCartVM);
        }

        
        public async Task<IActionResult> AddItemToShoppingCart(int id)
        {
            Movie movie = await _movieRepository.GetAsync(filter:x=>x.Id == id , includes: new[] { "Cinema","Producer","Category" } );

            if (movie != null) 
            {
                await _shoppingCart.AddItemToCart(movie);
                return RedirectToAction("ShoppingCart");
            }
            return RedirectToAction("Index","Movie");
        }

        public async Task<IActionResult> RemoveItemFromShoppingCart(int id)
        {
            var item = await _movieRepository.GetAsync(x=>x.Id == id);

            if (item != null)
            {
                await _shoppingCart.RemoveItemFromCart(item);
            }
            return RedirectToAction(nameof(ShoppingCart));
        }

        public async Task<IActionResult> CompleteOrder()
        {
            var items = _shoppingCart.GetShoppingCartItems();
            string userId = "";
            string userEmailAddress = "";

            await _orderService.StoreOrderAsync(items, userId, userEmailAddress);
            await _shoppingCart.ClearShoppingCartAsync();

            return View("OrderCompleted");
        }

    }
}
