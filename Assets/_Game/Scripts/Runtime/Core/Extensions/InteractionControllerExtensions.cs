using Game.Runtime.Character.Interfaces;
using Game.Runtime.Character.Components;

namespace Game.Runtime.Core.Extensions
{
    public static class InteractionControllerExtensions
    {
        public static ICarryingController GetCachedCarryController(this InteractionController controller)
        {
            // InteractionController'ın Character property'si üzerinden CarryingController'a erişim
            return controller.Character?.CarryingController;
        }
    }
}