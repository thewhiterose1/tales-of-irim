using Unity.VisualScripting;
using UnityEngine.UIElements;

public class JobsUIManager : UIManager
{
    public SliderInt FarmerSlider;

    public SliderInt EntertainerSlider;

    public SliderInt ArtisanSlider;

    public SliderInt MilitiaSlider;

    public Label IdlePopulation;

    public static JobsUIManager Instance;

    // Start is called before the first frame update
    void Start()
    {
        Instance = this;

        var root = GetRoot();
        FarmerSlider = root.Q<SliderInt>("Farmers");
        EntertainerSlider = root.Q<SliderInt>("Entertainers");
        ArtisanSlider = root.Q<SliderInt>("Artisans");
        MilitiaSlider = root.Q<SliderInt>("Militia");
        IdlePopulation = root.Q<Label>("IdlePopulation");

        FarmerSlider.value = 0;
        EntertainerSlider.value = 0;
        ArtisanSlider.value = 0;
        MilitiaSlider.value = 0;
        SetSliderLimits();
    }

    void Update()
    {
        SetSliderLimits();
        int FarmerMax = ColonyManager.Population - EntertainerSlider.value - ArtisanSlider.value - MilitiaSlider.value;
        int EntertainerMax = ColonyManager.Population - FarmerSlider.value - ArtisanSlider.value - MilitiaSlider.value;
        int ArtisanMax = ColonyManager.Population - FarmerSlider.value - EntertainerSlider.value - MilitiaSlider.value;
        int MilitiaMax = ColonyManager.Population - FarmerSlider.value - EntertainerSlider.value - ArtisanSlider.value;

        if (FarmerSlider.value > FarmerMax) FarmerSlider.value = FarmerMax;
        if (EntertainerSlider.value > EntertainerMax) EntertainerSlider.value = EntertainerMax;
        if (ArtisanSlider.value > ArtisanMax) ArtisanSlider.value = ArtisanMax;
        if (MilitiaSlider.value > MilitiaMax) MilitiaSlider.value = MilitiaMax;

        if (ColonyManager.Population != 0)
        {
            IdlePopulation.text = $"{GameManager.TranslationManager.GetTranslation("IDLE_POPULATION")}: ";
            IdlePopulation.text += ColonyManager.Population - FarmerSlider.value - ArtisanSlider.value - MilitiaSlider.value - EntertainerSlider.value;

        }
    }

    void SetSliderLimits()
    {
        FarmerSlider.highValue = ColonyManager.Population;
        EntertainerSlider.highValue = ColonyManager.Population;
        ArtisanSlider.highValue = ColonyManager.Population;
        MilitiaSlider.highValue = ColonyManager.Population;
    }
}
