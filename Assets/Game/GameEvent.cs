using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GameEvent", menuName = "Samsara/GameEvent", order = 0)]
public class GameEvent : ScriptableObject
{
    HashSet<GameEventListener> _listeners = new HashSet<GameEventListener>();

    public void Invoke()
    {
        foreach( var globalEventListener in _listeners) {
            globalEventListener.RaiseEvent();
        }
    }

    public void Register(GameEventListener gameEventListener) => _listeners.Add(gameEventListener);

    public void Deregister(GameEventListener gameEventListener) => _listeners.Remove(gameEventListener);
}
