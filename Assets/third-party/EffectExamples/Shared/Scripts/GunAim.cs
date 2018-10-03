using UnityEngine;

public class GunAim : MonoBehaviour
{
    public int borderBottom;
    public int borderLeft;
    public int borderRight;
    public int borderTop;
    private bool isOutOfBounds;

    private Camera parentCamera;

    private void Start()
    {
        parentCamera = GetComponentInParent<Camera>();
    }

    private void Update()
    {
        var mouseX = Input.mousePosition.x;
        var mouseY = Input.mousePosition.y;

        if (mouseX <= borderLeft || mouseX >= Screen.width - borderRight || mouseY <= borderBottom ||
            mouseY >= Screen.height - borderTop)
            isOutOfBounds = true;
        else
            isOutOfBounds = false;

        if (!isOutOfBounds) transform.LookAt(parentCamera.ScreenToWorldPoint(new Vector3(mouseX, mouseY, 5.0f)));
    }

    public bool GetIsOutOfBounds()
    {
        return isOutOfBounds;
    }
}