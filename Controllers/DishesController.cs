﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using youAreWhatYouEat.Models;

namespace youAreWhatYouEat.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DishesController : ControllerBase
    {
        private readonly ModelContext _context;

        public DishesController()
        {
            _context = new ModelContext();
        }



        public class GetDishesItem
        {
            public decimal id { get; set; }
            public string dis_name { get; set; }
            public decimal price { get; set; }
            public string description { get; set; }
        }


        // GET: api/Dishes
        [HttpGet]
        public async Task<ActionResult<List<GetDishesItem>>> GetDishes()
        {
            if (_context.Dishes == null)
            {
                return NotFound();
            }
            List<GetDishesItem> ret = new List<GetDishesItem>();

            await foreach (var item in _context.Dishes)
            {
                var dishesItem = new GetDishesItem();
                dishesItem.id = item.DishId;
                dishesItem.dis_name = item.DishName;
                dishesItem.price = item.DishPrice;
                dishesItem.description = item.DishDescription;
                ret.Add(dishesItem);
            }

            return Ok(ret);
        }

        [HttpGet("byname")]
        public async Task<ActionResult<GetDishesItem>> GetDish(string name)
        {
            if (_context.Dishes == null)
            {
                return NotFound();
            }

            try
            {
                var item = await _context.Dishes.Where(e => e.DishName == name).FirstAsync();
                GetDishesItem ret = new GetDishesItem();
                ret.id = item.DishId;
                ret.dis_name = item.DishName;
                ret.price = item.DishPrice;
                ret.description = item.DishDescription;
                return ret;
            }
            catch (Exception ex)
            {
                return NotFound();
            }
        }

        // GET: api/Dishes/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Dish>> GetDish(decimal id)
        {
            if (_context.Dishes == null)
            {
                return NotFound();
            }
            var dish = await _context.Dishes.FindAsync(id);

            if (dish == null)
            {
                return NotFound();
            }

            return dish;
        }


        // POST: api/Dishes
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost("UpdateDish")]
        public async Task<ActionResult> PostDish(GetDishesItem dish)
        {
            if (_context.Dishes == null)
            {
                return Problem("Entity set 'ModelContext.Dishes'  is null.");
            }

            var dm = await _context.Dishes.FindAsync(dish.id);

            dm.DishDescription = dish.description;
            dm.DishName = dish.dis_name;
            dm.DishPrice = dish.price;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return NotFound();
            }

            return Ok();
        }

        // POST: api/Dishes
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Dish>> PostDish(Dish dish)
        {
            if (_context.Dishes == null)
            {
                return Problem("Entity set 'ModelContext.Dishes'  is null.");
            }
            _context.Dishes.Add(dish);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (DishExists(dish.DishId))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetDish", new { id = dish.DishId }, dish);
        }

        public class DishOrderItem
        {
            public string? dish_name { get; set; } = string.Empty;
            public string? status { get; set; } = null;
        }

        public class DishOrderListItem
        {
            public string? order_id { get; set; } = string.Empty;
            public string? order_status { get; set; } = null;
            public List<DishOrderItem>? dish { get; set; } = new List<DishOrderItem>();
        }



        // GET: api/Dishes/OrderList
        [HttpGet("OrderList")]
        public async Task<ActionResult<List<DishOrderListItem>>> GetOrderList()
        {
            var ret = new List<DishOrderListItem>();
            var dl = await _context.Dishorderlists.Include(e => e.Dish).ToListAsync();
            foreach (var d in dl.GroupBy(e => e.OrderId))
            {
                string id = d.Key;
                DishOrderListItem dishOrderListItem = new DishOrderListItem();
                dishOrderListItem.order_id = id;
                dishOrderListItem.order_status = _context.Orderlists.Where(e => e.OrderId == id).First().OrderStatus;
                foreach (var item in d.AsEnumerable())
                {
                    DishOrderItem ditem = new DishOrderItem();
                    ditem.status = item.DishStatus;
                    ditem.dish_name = item.Dish.DishName;
                    dishOrderListItem.dish.Add(ditem);
                }
                ret.Add(dishOrderListItem);
            }
            return Ok(ret);
        }

        // DELETE: api/Dishes/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDish(decimal id)
        {
            if (_context.Dishes == null)
            {
                return NotFound();
            }
            var dish = await _context.Dishes.FindAsync(id);
            if (dish == null)
            {
                return NotFound();
            }

            _context.Dishes.Remove(dish);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool DishExists(decimal id)
        {
            return (_context.Dishes?.Any(e => e.DishId == id)).GetValueOrDefault();
        }
    }
}