using Game.Runtime.Character.Interfaces;
using Game.Runtime.Items;
using Game.Runtime.Character.Components;
using Game.Runtime.Items.Data;


namespace Game.Runtime.Core.Extensions
{
    public static class CarryingControllerExtensions
    {
        public static ItemType CurrentItemType(this ICarryingController controller)
        {
            if (controller is StackingCarryController stackingController)
            {
                return stackingController.CurrentItemType;
            }
            return ItemType.None;
        }

        public static bool IsFull(this ICarryingController controller)
        {
            if (controller is StackingCarryController stackingController)
            {
                return stackingController.IsFull;
            }
            return false;
        }

        public static Item RemoveTopItem(this ICarryingController controller)
        {
            if (controller is StackingCarryController stackingController)
            {
                return stackingController.RemoveTopItem();
            }
            return null;
        }
    }
}