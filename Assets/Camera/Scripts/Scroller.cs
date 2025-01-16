using UnityEngine;

/// <summary>
/// Provides level scrolling by width.
/// </summary>
public class Scroller : MonoBehaviour
{
    [Tooltip("Scrolling speed"), SerializeField]
    private float _scrollingSpeed;

    [Tooltip("Zones on the sides in which to start scrolling in dragging mode (as ratio)"), SerializeField]
    private float _offsetAreaSize;

    [Tooltip("Background object renderer"), SerializeField]
    private Renderer _backgroundRenderer;

    private Vector2 _touchStartPosition;    // Touch start point for offset calculation

    private float _camLeftBorder, _camRightBorder;  // Left and right camera borders in ox axis

    public static bool IsDraggingMode { get; set; } = false; // To switch control mode

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        // Recalculation of the scroll start zone in pixels in dragging mode
        _offsetAreaSize *= Screen.width;

        if (_backgroundRenderer != null)
        {
            // Calculate ox axis borders for the camera, based on its width and background size
            float camWidthInUnits = Camera.main.aspect * Camera.main.orthographicSize;
            _camLeftBorder = _backgroundRenderer.bounds.min.x + camWidthInUnits;
            _camRightBorder = _backgroundRenderer.bounds.max.x - camWidthInUnits;
        }
        else
            Debug.Log("Level size not detected, camera will be static");
    }

    // Update is called once per frame
    private void Update()
    {
        if (Input.touchCount > 0)
        {
             Touch touch = Input.GetTouch(0);

             switch (touch.phase)
             {
                case TouchPhase.Began:
                    _touchStartPosition = touch.position;   // Save touch start point
                    break;
                case TouchPhase.Moved:
                    // Swipe level if not dragging item
                    if (!IsDraggingMode)
                        SwipeLevel(touch.position);

                    break;
             }

            // Check for scrolling areas in dragging mode based only on touch position
            if (IsDraggingMode)
                FollowItem(touch.position);
        }
    }

    /// <summary>
    /// Swipe level by width
    /// </summary>
    /// <param name="currentPosition">Current touch position</param>
    private void SwipeLevel(Vector2 currentPosition)
    {
        // Scroll speed reversed from touch direction
        float delta = currentPosition.x - _touchStartPosition.x > .0f ? -_scrollingSpeed : _scrollingSpeed;
        MoveCamera(delta);
    }

    /// <summary>
    /// Moving camera following touch in scroll areas
    /// </summary>
    /// <param name="currentPosition">Current touch position</param>
    private void FollowItem(Vector2 currentPosition)
    {
        float delta = .0f;  // If the touch is not in the scroll zones, there is no offset

        // Checking zones on the left and right
        if (currentPosition.x < _offsetAreaSize)
            delta = -_scrollingSpeed;
        else if (currentPosition.x > Screen.width - _offsetAreaSize)
            delta = _scrollingSpeed;

        MoveCamera(delta);
    }

    /// <summary>
    /// Move camera by ox axis
    /// </summary>
    /// <param name="delta">Offset by ox axis</param>
    private void MoveCamera(float delta)
    {
        if (Mathf.Approximately(delta, .0f))
            return;

        Camera.main.transform.Translate(delta * Time.deltaTime, .0f, .0f);
        KeepCameraOnLevel();    // Check level borders
    }

    /// <summary>
    /// Keeping camera in level area
    /// </summary>
    private void KeepCameraOnLevel()
    {
        // If the camera goes out of width bounds,return it
        if (transform.position.x < _camLeftBorder)
            transform.position = new Vector3(_camLeftBorder, transform.position.y, transform.position.z);
        else if (transform.position.x > _camRightBorder)
            transform.position = new Vector3(_camRightBorder, transform.position.y, transform.position.z);
    }
}
