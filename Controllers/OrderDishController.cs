﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using youAreWhatYouEat.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace youAreWhatYouEat.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderDishController : ControllerBase
    {
        private readonly ModelContext _context;
        public OrderDishController()
        {
            _context = new ModelContext();
        }

        public class DishInfo
        {
            public string? dish_name { get; set; }
            public string? dish_status { get; set; }
            public decimal? dish_price { get; set; }
            public string? dish_picture { get; set; }
            public int? dish_num { get; set; }
            public int? table_id { get; set; }
        }

        public class OrderDishInfo2
        {
            public List<DishInfo> dish_info = new List<DishInfo>();
        }

        // GET 获取订单的菜品信息
        [HttpGet("GetOrderDishInfo")]
        public async Task<ActionResult<OrderDishInfo2>> GetOrderDishInfo(string? order_id)
        {
            if (order_id == null) return BadRequest();
            var orderdish = await _context.Orderlists
                .Include(o => o.Dishorderlists)
                    .ThenInclude(d => d.Dish)
                .FirstOrDefaultAsync(o => o.OrderId == order_id);
            if (orderdish == null) return NotFound();

            OrderDishInfo2 orderDishInfo = new OrderDishInfo2();
            List<DishInfo> infos = new List<DishInfo>();
            foreach (var dish in orderdish.Dishorderlists)
            {
                bool tag = false;
                for (int i = 0; i < infos.Count; i++)
                {
                    if (infos[i].dish_name == dish.Dish.DishName && infos[i].dish_status == dish.DishStatus)
                    {
                        infos[i].dish_num++;
                        tag = true;
                        break;
                    }
                }
                if (tag) continue;

                DishInfo dishInfo = new DishInfo();
                dishInfo.dish_name = dish.Dish.DishName;
                dishInfo.dish_price = dish.FinalPayment;
                dishInfo.dish_status = dish.DishStatus;
                dishInfo.dish_num = 1;
                dishInfo.dish_picture = System.Configuration.ConfigurationManager.AppSettings["ImagesUrl"] + "dishes/dish_" + dish.DishId.ToString() + ".png";
                dishInfo.table_id = Convert.ToInt32(orderdish.TableId);
                infos.Add(dishInfo);
            }
            orderDishInfo.dish_info = infos;
            return Ok(orderDishInfo);
        }

        public class DishInfo2
        {
            public int dish_id { get; set; }
            public string? dish_name { get; set; }
            public decimal? dish_price { get; set; }
            public string? dish_picture { get; set; }
            public decimal? dish_rate { get; set; }
            public string? dish_description { get; set; }
            public decimal? dish_discount { get; set; }
        }

        public class CategoryDishInfo
        {
            public List<DishInfo2> dish_havethetag = new List<DishInfo2>();
        }

        // GET 获取某一类所有菜品
        [HttpGet("GetCategoryDishes")]
        public async Task<ActionResult<CategoryDishInfo>> GetCategoryDishes(string? dish_tag, int? promotion_id)
        {
            if (dish_tag == null) return NotFound();
            var tag = await _context.Dishtags
                .Include(d => d.Dishes)
                    .ThenInclude(dish => dish.CommentOnDishes)
                .Include(d => d.Dishes)
                    .ThenInclude(dish => dish.Hasdishes)
                .FirstOrDefaultAsync(d => d.DtagName == dish_tag);
            if (tag == null) return NotFound();

            CategoryDishInfo info = new CategoryDishInfo();
            List<DishInfo2> list = new List<DishInfo2>();
            foreach (var dish in tag.Dishes)
            {
                DishInfo2 d = new DishInfo2();
                d.dish_id = Convert.ToInt32(dish.DishId);
                d.dish_name = dish.DishName;
                d.dish_price = dish.DishPrice;
                d.dish_description = dish.DishDescription;
                d.dish_picture = System.Configuration.ConfigurationManager.AppSettings["ImagesUrl"] + "dishes/dish_" + dish.DishId.ToString() + ".png";

                d.dish_discount = 1;
                foreach(var pro in dish.Hasdishes)
                {
                    if (pro.PromotionId == promotion_id)
                    {
                        d.dish_discount = pro.Discount;
                        break;
                    }
                }
                
                decimal rate = 0;
                decimal count = 0;
                foreach(var cmt in dish.CommentOnDishes)
                {
                    if (cmt.Stars == null) continue;
                    rate += Convert.ToInt32(cmt.Stars);
                    count++;
                }
                if (count == 0) rate = 0;
                else rate = rate / count;
                d.dish_rate = rate;
                list.Add(d);
            }

            info.dish_havethetag = list;
            return Ok(info);
        }

        public class PriceInfo
        {
            public decimal? orderTotalPrice { get; set; }
        }

        // GET 获取订单总价格
        [HttpGet("GetOrderPrice")]
        public async Task<ActionResult<PriceInfo>> GetOrderPrice(string? order_id)
        {
            if (order_id == null) return BadRequest();
            var order = await _context.Orderlists
                .Include(o => o.Dishorderlists)
                .FirstOrDefaultAsync(o => o.OrderId == order_id);
            if (order == null) return NotFound();

            PriceInfo info = new PriceInfo();
            decimal price = 0;
            foreach(var item in order.Dishorderlists)
            {
                price += item.FinalPayment;
            }
            info.orderTotalPrice = price;

            return Ok(info);
        }

        public class StatusInfo
        {
            public string? order_status { get; set; }
        }

        // GET 获取订单支付状态
        [HttpGet("GetOrderStatus")]
        public async Task<ActionResult<StatusInfo>> GetOrderStatus(string? order_id)
        {
            if (order_id == null) return BadRequest();
            var order = await _context.Orderlists
                .FirstOrDefaultAsync(o => o.OrderId == order_id);
            if (order == null) return NotFound();

            StatusInfo info = new StatusInfo();
            info.order_status = order.OrderStatus;

            return Ok(info);
        }

        public class PromotionDish
        {
            public decimal dish_id { get; set; }
            public string? dish_name { get; set; }
            public decimal dish_price { get; set; }
            public string? dish_description { get; set; }

            public PromotionDish(Dish d)
            {
                dish_id = d.DishId;
                dish_name = d.DishName;
                dish_price = d.DishPrice;
                dish_description = d.DishDescription;
            }
        }

        public class PromotionDishRecord
        {
            public PromotionDish? dish { get; set; } = null!;
            public decimal discount { get; set; } = 1.0M;
        }

        public class PromotionRecord
        {
            public decimal promotion_id { get; set; }
            public string? description { get; set; } = null!;
            public List<PromotionDishRecord> dishes { get; set; } = new List<PromotionDishRecord>();
        }

        // Get 获取正在进行的促销活动
        [HttpGet("GetPromotion")]
        public async Task<ActionResult<PriceInfo>> GetPromotion()
        {
            if (_context.Promotions == null)
            {
                return NotFound();
            }

            List<PromotionRecord> ret = new List<PromotionRecord>();
            await foreach (var p in _context.Promotions.Include(e => e.Hasdishes).ThenInclude(e => e.Dish).AsAsyncEnumerable())
            {
                if (p.StartTime > DateTime.Now || p.EndTime < DateTime.Now) continue;
                PromotionRecord pr = new PromotionRecord();
                pr.promotion_id = p.PromotionId;
                pr.description = p.Description;
                var d = p.Hasdishes;
                foreach (var di in d)
                {
                    PromotionDishRecord dt = new PromotionDishRecord();
                    if (di.Discount != null) dt.discount = (decimal)di.Discount; else dt.discount = 0.0M;
                    dt.dish = new PromotionDish(di.Dish);
                    pr.dishes.Add(dt);
                }
                ret.Add(pr);
            }
            return Ok(ret);
        }

        public class PostOrderInfo
        {
            public Dictionary<string, dynamic> dishes_info = new Dictionary<string, dynamic>();

            public PostOrderInfo()
            {
                dishes_info.Add("dish_id", 0);
                dishes_info.Add("dish_num", 0);
            }
        }

        public class ReturnOrder
        {
            public string? order_id { get; set; }
        }

        // POST 提交订单
        [HttpPost("PostOrder")]
        public async Task<ActionResult<ReturnOrder>> PostOrder(PostOrderInfo p)
        {
            if (p.dishes_info["dish_id"] == null || p.dishes_info["dish_num"] == null) return BadRequest();
            var orders = await _context.Orderlists
                .Select(o => o.OrderId)
                .ToListAsync();

            Orderlist order = new Orderlist();
            order.OrderStatus = "待处理";
            order.CreationTime = DateTime.Now;

            Random random = new Random();
            string order_id = "";
            do
            {
                order_id = "";
                for (int i = 0; i < 11; i++)
                {
                    int r = random.Next(0, 62);
                    if (r < 10) order_id += r.ToString();
                    else if (r < 36) order_id += (char)(97 + r - 10);
                    else order_id += (char)(65 + r - 36);
                }
            } while (orders.IndexOf(order_id) != -1);
            order.OrderId = order_id;

            try
            {
                _context.Orderlists.Add(order);
                await _context.SaveChangesAsync();
            } catch (Exception ex)
            {
                return BadRequest(ex);
            }

            for (int k = 0; k < p.dishes_info["dish_num"]; k++) {
                Dishorderlist dish_order = new Dishorderlist();
                dish_order.OrderId = order_id;
                dish_order.DishId = p.dishes_info["dish_id"];
                dish_order.DishStatus = "待处理";

                string dish_order_id = "";
                var dish_orders = new List<string>();
                do
                {
                    dish_orders = await _context.Dishorderlists
                        .Select(d => d.DishOrderId)
                        .ToListAsync();

                    dish_order_id = "";
                    for (int i = 0; i < 11; i++)
                    {
                        int r = random.Next(0, 62);
                        if (r < 10) dish_order_id += r.ToString();
                        else if (r < 36) dish_order_id += (char)(97 + r - 10);
                        else dish_order_id += (char)(65 + r - 36);
                    }
                } while (dish_orders.IndexOf(dish_order_id) != -1);
                dish_order.DishOrderId = dish_order_id;

                try
                {
                    _context.Dishorderlists.Add(dish_order);
                    await _context.SaveChangesAsync();
                } catch (Exception ex)
                {
                    return BadRequest(ex);
                }
            }

            return Ok(order_id);
        }

        public class PostTableInfo
        {
            public int? table_id { get; set; }
        }

        // POST 更新桌面状态信息
        [HttpPost("PostUpdateTable")]
        public async Task<ActionResult> PostUpdateTable(PostTableInfo p)
        {
            if (p.table_id == null) return BadRequest();
            var table = await _context.Dinningtables
                .FirstOrDefaultAsync(t => t.TableId == p.table_id);
            if (table == null) return NotFound();

            table.Occupied = "否";
            try
            {
                await _context.SaveChangesAsync();
                return Ok();
            } catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        public class DishCommentInfo
        {
            public decimal? rate { get; set; }
            public string? content { get; set; }
            public int? dish_id { get; set; }
            public string? user_name { get; set; }
        }

        // POST 提交菜品评价
        [HttpPost("PostDishComment")]
        public async Task<ActionResult> PostDishComment(DishCommentInfo p)
        {
            if (p.dish_id == null || p.user_name == null) return BadRequest();
            CommentOnDish cmt = new CommentOnDish();
            cmt.Stars = p.rate;
            cmt.DishId = Convert.ToDecimal(p.dish_id);
            cmt.UserName = p.user_name;
            cmt.CommentContent = p.content;
            cmt.CommentTime = DateTime.Now;

            Random random = new Random();
            var cod = new List<string>();
            string cid = "";
            do
            {
                cod = await _context.CommentOnDishes
                    .Select(d => d.CommentId)
                    .ToListAsync();

                cid = "";
                for (int i = 0; i < 16; i++)
                {
                    int r = random.Next(0, 62);
                    if (r < 10) cid += r.ToString();
                    else if (r < 36) cid += (char)(97 + r - 10);
                    else cid += (char)(65 + r - 36);
                }
            } while (cod.IndexOf(cid) != -1);
            cmt.CommentId = cid;

            try
            {
                _context.CommentOnDishes.Add(cmt);
                await _context.SaveChangesAsync();
                return Ok();
            } catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        public class ServiceCommentInfo
        {
            public decimal? rate { get; set; }
            public string? content { get; set; }
            public string? user_name { get; set; }
        }

        // POST 提交服务评价
        [HttpPost("PostServiceComment")]
        public async Task<ActionResult> PostServiceComment(ServiceCommentInfo p)
        {
            if (p.user_name == null) return BadRequest();
            CommentOnService cms = new CommentOnService();
            cms.Stars = p.rate;
            cms.UserName = p.user_name;
            cms.CommentContent = p.content;
            cms.CommentTime = DateTime.Now;

            Random random = new Random();
            var cos = new List<string>();
            string cid = "";
            do
            {
                cos = await _context.CommentOnServices
                    .Select(d => d.CommentId)
                    .ToListAsync();

                cid = "";
                for (int i = 0; i < 16; i++)
                {
                    int r = random.Next(0, 62);
                    if (r < 10) cid += r.ToString();
                    else if (r < 36) cid += (char)(97 + r - 10);
                    else cid += (char)(65 + r - 36);
                }
            } while (cos.IndexOf(cid) != -1);
            cms.CommentId = cid;

            try
            {
                _context.CommentOnServices.Add(cms);
                await _context.SaveChangesAsync();
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }
    }
}