using System.Drawing;

namespace GSCEditor
{
    interface UniversalObject
    {
        //строки для обработки
        public int getMinY(); // нижняя строка
        public int getMaxY(); // верхняя строка    
        public Sides getActualSides(int y); //пары левых и правых границ для строки

        // реализация геометрических преобразований
        public void move(float dx, float dy); 
        public void rotate(float angle, PointF relatePoint);
        public void scale(float s, PointF relatePoint);
        public void reset();
        public void mirror(PointF origin);
        public PointF getCenter(); // центр объекта
        private void renderText(Graphics g, string text) // подпись объекта
        {
            PointF origin = getCenter();
            Font drawFont = new Font("Arial", 16);
            SolidBrush drawBrush = new SolidBrush(Color.DarkOliveGreen);
            g.DrawString(text, drawFont, drawBrush, origin);
        }

        // обычная отрисовка
        public void render(Graphics g, Pen pen); 
        // отрисовка с подписью
        public void render(Graphics g, Pen pen, string v) { 
            render(g, pen);
            renderText(g, v);
        }
        public bool isInside(int x, int y); // находится ли точка внутри объекта, необходимо для захвата
    }
}
