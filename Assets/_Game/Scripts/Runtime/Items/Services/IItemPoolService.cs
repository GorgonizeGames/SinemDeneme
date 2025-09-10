using Game.Runtime.Items.Data;

namespace Game.Runtime.Items.Services
{
    public interface IItemPoolService
    {
        Item GetItem(ItemType type);
        void ReturnItem(Item item);
    }
}
