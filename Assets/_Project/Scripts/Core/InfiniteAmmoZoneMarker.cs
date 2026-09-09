using UnityEngine;

public class InfiniteAmmoZoneMarker : MonoBehaviour
{
    private void Awake() => InfiniteAmmoZone.Enter();
    private void OnDestroy() => InfiniteAmmoZone.Exit(); 
}
