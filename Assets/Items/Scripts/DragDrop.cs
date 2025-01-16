using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Provides drag&drop mechanics for a single item.
/// </summary>
[RequireComponent(typeof(Renderer))]
public class DragDrop : MonoBehaviour
{
    [Tooltip("Item falling speed"), SerializeField]
    private float _dropSpeed;
    
    [Tooltip("Bottom thickness for a little 3d effect"), SerializeField]
    private float _bottomIntersectCoeff;

    [Tooltip("Leaving time after destroyed"), SerializeField]
    private float _leavingTime;

    [Tooltip("Interaction sounds: picking up, dropping and throwing out"), SerializeField]
    private List<AudioClip> _sounds = new List<AudioClip>(3);

    private AudioSource _audioSource;   // Plays sounds from list
    private Animator _animator;
    private Renderer _renderer;
    private bool _isDragging = false;   // To determine if an item is being dragged
    private Vector3 _destinationPos = Vector3.zero; // Position where to place the falling item

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        _animator = GetComponent<Animator>();
        _renderer = GetComponent<Renderer>();
        _audioSource = GetComponent<AudioSource>();

        DetectColliderBelow(); // Initial positioning
    }

    // Update is called once per frame
    private void Update()
    {
        // For dragging mode move the item following the pointer
        if (_isDragging && Input.touchCount > 0)
        {
           // Vector2 mouseOffset = Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position;
            Vector2 mouseOffset = Camera.main.ScreenToWorldPoint(Input.GetTouch(0).position) - transform.position;
            transform.Translate(mouseOffset);
        }
        // Otherwise, the item must be placed at the calculated position
        else
        {
            float dropStep = _dropSpeed * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, _destinationPos, dropStep);
        }
    }

    private void OnMouseDown()
    {
        // After click on item, picking up it
        _isDragging = true;
        _renderer.sortingOrder = LevelConfig.OrdersPerLayer + 1; // Carried item to the foreground
        PlaySound("PickUp");
        SwitchDragMode();   // Dragging mode on
        Debug.Log("Pick " + gameObject.name);
    }

    private void OnMouseUp()
    {
        // After click off, dropping item
        _isDragging = false;    
        PlaySound("Drop");
        SwitchDragMode(); // Dragging mode off 
        DetectColliderBelow();  // Сalculating where it will fall
        Debug.Log("Drop " + gameObject.name);
    }

    /// <summary>
    /// Provides a change of animation and control mode
    /// </summary>
    private void SwitchDragMode()
    {
        if (_animator != null)
            _animator.SetBool("IsDragging", _isDragging);

        Scroller.IsDraggingMode = _isDragging; // Determines how the touch will be processed
    }

    /// <summary>
    /// Ray-casting from the bottom of an object looking for surfaces to place it on.
    /// When detected, a new position and order on the layer are calculated.
    /// </summary>
    private void DetectColliderBelow()
    {
        // Calculate start ray point
        Vector2 objBottomPos = new Vector2(_renderer.bounds.center.x, _renderer.bounds.min.y + _bottomIntersectCoeff);
        
        // Ray-casting down for layer with environment colliders
        RaycastHit2D hitInfo = Physics2D.Raycast(objBottomPos, Vector2.down, Camera.main.orthographicSize * 2.0f, LayerMask.GetMask("Default"));
        
        if (hitInfo.collider == null)
            return;

        // Hollow box-type containers have their own logic
        if (hitInfo.collider.gameObject.tag == "Container")
        {
            // Position the object at the top center
            _destinationPos.x = hitInfo.collider.bounds.center.x;
            _destinationPos.y = hitInfo.collider.bounds.max.y;

            if (_animator != null)
                _animator.SetTrigger("PutAway"); // Disappearing animation

            PlaySound("PutAway");
            Destroy(gameObject,_leavingTime); // A little later the item moves away
        }
        // If there is a place to post item on it
        else
        {
            // Сalculate the position taking into account that the bottom of the object should be at the point of contact
            _destinationPos.x = hitInfo.point.x;
            _destinationPos.y = hitInfo.point.y + _renderer.bounds.size.y / 2.0f - _bottomIntersectCoeff;

            // The z position is used to allow the touch to interact with an object in the foreground as it intersects with others
            _destinationPos.z = hitInfo.point.y - Camera.main.orthographicSize;

            // Next, we calculate the order of the item on the layer for correct rendering
            // How many units are there per one order
            float thicknessPerOrder = Camera.main.orthographicSize * 2.0f / (float)LevelConfig.OrdersPerLayer;
            // Order number at the starting point (for y = 0)
            float middleOrder = (float)LevelConfig.OrdersPerLayer / 2.0f;
            // Now calculate the order depending on the height position of the item
             _renderer.sortingOrder = (int)(middleOrder + (-hitInfo.point.y) / thicknessPerOrder);
        }
    }

    /// <summary>
    /// Setting audio source and play sound.
    /// </summary>
    /// <param name="name">Name of sound (PickUp, Drop or PutAway)</param>
    private void PlaySound(string name)
    {
        if (_audioSource == null)
            return;

        // Depending on the choice, set the source
        switch (name)
        {
            case "PickUp":
                _audioSource.clip = _sounds[0];
                break;
            case "Drop":
                _audioSource.clip = _sounds[1];
                break;
            case "PutAway":
                _audioSource.clip = _sounds[2];
                break;  
            default:
                _audioSource.clip = null;
                break;
        }

        if (_audioSource.clip != null)
            _audioSource.Play();
    }
}