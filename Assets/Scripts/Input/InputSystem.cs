using Unity.Entities;
using UnityEngine;

public partial class InputSystem : SystemBase
{

    private Controls controls;
    protected override void OnCreate()
    {
        if (!SystemAPI.TryGetSingleton(out InputComponent input))
        {
            EntityManager.CreateEntity(typeof(InputComponent));
        }
        
        controls = new Controls();
        controls.Enable();
    }

    protected override void OnUpdate()
    {
        Vector2 movement = controls.BaseActions.Move.ReadValue<Vector2>();
        Vector2 mousePosition = controls.BaseActions.MousePos.ReadValue<Vector2>();
        bool fire = controls.BaseActions.Shoot.IsPressed();
        
        SystemAPI.SetSingleton(new InputComponent
        {
            movement = movement,
            mousePosition = mousePosition,
            fire = fire
        });
    }
}
