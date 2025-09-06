using Game.Runtime.Game;
using Game.Runtime.Input;
using UnityEngine;

namespace Game.Runtime.Core.Interfaces
{
    public interface ICarrier
    {
        Transform Transform { get; }
        Transform CarryPoint { get; }
    }
}