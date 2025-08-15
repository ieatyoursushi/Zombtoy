using UnityEngine;
using UnityEngine.UI;

public class ZombieCountBinder : MonoBehaviour
{
    [SerializeField] private Text countText;
    [SerializeField] private string prefix = "Zombies: ";

    private void Awake()
    {
        if (countText == null) countText = GetComponent<Text>();
    }

    private void OnEnable()
    {
        GameEvents.OnEnemyCountChanged += HandleCountChanged;
        // Initial fill
        HandleCountChanged(EnemyManager.GetActiveCount());
    }

    private void OnDisable()
    {
        GameEvents.OnEnemyCountChanged -= HandleCountChanged;
    }

    private void HandleCountChanged(int active)
    {
        if (countText == null) return;
        countText.text = $"{prefix}{active}";
    }
}
