using Order.API.Models.Entities;
using Order.API.Models.Enums;

namespace Order.API.Models.ViewModels;

public class CreateOrderVM
{
    public Guid BuyerId { get; set; }
    public ICollection<CreateOrderItemVM> OrderItems { get; set; }
}
public class CreateOrderItemVM
{
    public string ProductId { get; set; }
    public int Count { get; set; }
    public decimal Price { get; set; }

}