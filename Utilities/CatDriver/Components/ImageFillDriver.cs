namespace Cat.Utilities
{
    using UnityEngine;
    using UnityEngine.UI;

    public class ImageFillDriver : CatDriver<float, float>
    {
        [SerializeField] private Image image;
        public override void Drive(float p1, float p2)
        {
            image.fillAmount = p1 / p2;
        }
    }

}