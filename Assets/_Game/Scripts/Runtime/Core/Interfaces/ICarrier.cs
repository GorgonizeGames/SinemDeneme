using Game.Runtime.Character.AI.Factory;
using Game.Runtime.Character.Components;
using Game.Runtime.Character.Interfaces;
using Game.Runtime.Character.Motor;
using Game.Runtime.Character.States;
using Game.Runtime.Core.Interfaces;
using Game.Runtime.Core.StateMachine;
using Game.Runtime.Items.Interfaces;
using UnityEngine;

namespace Game.Runtime.Core.Interfaces
{
    public interface ICarrier
    {
        Transform Transform { get; }
        Transform CarryPoint { get; }
    }
}