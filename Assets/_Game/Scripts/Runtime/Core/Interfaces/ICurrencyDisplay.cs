using UnityEngine;
using System.Threading.Tasks;

namespace Game.Runtime.Core.Interfaces
{
    public interface ICurrencyDisplay
    {
        void UpdateMoney(float amount, bool animate = true);
        Task PlayMoneyGainAnimation(Vector3 sourcePosition, float amount);
    }
}