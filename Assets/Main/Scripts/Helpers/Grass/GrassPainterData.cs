using UnityEngine.UIElements;

namespace Main.Scripts.Helpers.Grass
{
    public struct GrassPainterData
    {
        public GrassPainterForce Force;
        public VisualElement element;

        public GrassPainterData(GrassPainterForce force, VisualElement element)
        {
            this.Force = force;
            this.element = element;
        }
    }
}