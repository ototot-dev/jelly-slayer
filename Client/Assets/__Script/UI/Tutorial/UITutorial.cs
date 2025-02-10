using UnityEngine;

public class UITutorial : MonoBehaviour
{
    public GameObject _objPanel;
    public GameObject _objButton;

    public bool _isViewPanel = true;
    float _viewTime = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _viewTime = 3;
        _isViewPanel = true;
        _objPanel.SetActive(true);
        _objButton.SetActive(false);
    }
    private void OnEnable()
    {
        _viewTime = 3;
        _isViewPanel = true;
        _objPanel.SetActive(true);
        _objButton.SetActive(false);
    }
    public void OnClickHelp() 
    {
        _viewTime = 3;
        _isViewPanel = true;
        _objPanel.SetActive(true);
        _objButton.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (_isViewPanel == false)
            return;

        _viewTime -= Time.deltaTime;
        if (_viewTime <= 0) 
        {
            _isViewPanel = false;
            _objPanel.SetActive(false);
            _objButton.SetActive(true);
        }
    }
}
