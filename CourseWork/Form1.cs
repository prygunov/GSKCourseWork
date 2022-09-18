using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace GSCEditor
{
    public partial class Form1 : Form
    {
        Bitmap myBitmap;
        Graphics g;
        Pen pen = new Pen(Color.Black, 1);
        Pen arrowPen = new Pen(Color.Black, 1);

        List<UniversalObject> objects = new List<UniversalObject>();
        int selectedIndex = -1;
        List<PointF> inputPoints = new List<PointF>();

        Point lastMousePos = new Point();

        private void changeObject(object sender, MouseEventArgs e)
        {
            if (getMode() == 1 & selectedIndex != -1)
            {
                UniversalObject polygon = objects[selectedIndex];
                PointF relatePoint = new PointF(e.X, e.Y);
                if (e.Button == MouseButtons.Left)
                    polygon.move(e.X - lastMousePos.X, e.Y - lastMousePos.Y);
                if (e.Delta != 0)
                {
                    if (Control.ModifierKeys == Keys.Alt)
                    {
                        int grad = 45;
                        float rad = grad * 0.0174533f; // 0.0174.. радиан одного градуса
                        polygon.rotate(e.Delta / 120f * rad, relatePoint);
                    }
                    else
                    {
                        polygon.scale(1f + e.Delta / 1200f, polygon.getCenter());
                    }
                }

                render();

                pictureBox1.Refresh();
                lastMousePos = e.Location;
            }
        }

        public Form1()
        {
            InitializeComponent();
            myBitmap = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            g = Graphics.FromImage(myBitmap);
            pictureBox1.MouseWheel += new MouseEventHandler(changeObject);
            pictureBox1.MouseMove += new MouseEventHandler(changeObject);
            pictureBox1.Image = myBitmap;

            arrowPen.CustomEndCap = new AdjustableArrowCap(6, 6); // стрела

            comboBox1.SelectedIndex = 0;
            comboBox4.SelectedIndex = 0;
            comboBox5.SelectedIndex = 0;
            comboBox7.SelectedIndex = 0;
        }

        // заполнение списка вершин
        private void inputVertex(MouseEventArgs e)
        {
            Point NewP = new Point() { X = e.X, Y = e.Y };
            inputPoints.Add(NewP);
            renderInput();
            if (getMode() == 0)
            {
                if (e.Button == MouseButtons.Right && inputPoints.Count > 1) // Конец ввода
                {
                    // если нажата правая кнопка мыши и точек более 1 то создаем примитив
                    objects.Add(new Primitive(inputPoints, getColor()));
                    render();
                    updateBoxes();
                    inputPoints.Clear();
                }
            }
            else
            {
                if (e.Button == MouseButtons.Right && inputPoints.Count > 1) // Конец ввода
                {
                    // создание кривой
                    Primitive p = new Primitive(inputPoints, getColor());
                    p.setMode(2);
                    objects.Add(p);
                    render();
                    updateBoxes();
                    inputPoints.Clear();
                }
               
               
            }
        }

        // захват объекта, возвращает индекс
        int getSelectedIndex(int x, int y)
        {
            int i = 0;
            foreach (UniversalObject polygon in objects)
            {
                if (polygon.isInside(x, y))
                {
                    return i;
                }
                i++;
            }
            
            return -1;
        }

        //включает и отключает кнопки
        private void updateButtonState() {
           
            if (selectedIndex != -1) {
                button1.Enabled = true;
                button4.Enabled = true;
            }else{
                button4.Enabled = false;
                button1.Enabled = false;
            }
                
        }

        // Обработчик события нажатия кнопки мыши в поле вывода
        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            lastMousePos = e.Location;
            switch (getMode())
            {
                case 2:
                case 0:
                    //ввод вершин объекта
                    inputVertex(e);
                    break;
                case 1:
                    //захват для работы с объектом
                    int index = getSelectedIndex(e.X, e.Y);
                    if (index != -1)
                    {
                        selectedIndex = index;
                        g.DrawEllipse(new Pen(Color.Blue), e.X - 2, e.Y - 2, 5, 5);
                    }
                    else selectedIndex = -1;
                    
                    break;
                case 3:
                    //добавление заранее заданных примитивов
                    addPrimitive(e.X, e.Y);
                    break;
            }
            updateButtonState();
            pictureBox1.Image = myBitmap;
        }


        // Обработчик события выбора цвета
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            Color specificColor = getColor();
            pen.Color = specificColor;
            arrowPen.Color = specificColor;
        }

        // возвращает выбранный цвет
        Color getColor()
        {
            switch (comboBox5.SelectedIndex)    
            {
                case 1:
                    return Color.Red;
                case 2:
                    return Color.Green;
                case 3:
                    return Color.Blue;
                default:
                    return Color.Black;
            }
        }

        // очистка поля
        private void button2_Click(object sender, EventArgs e)
        {
            pictureBox1.Image = myBitmap;
            g.Clear(pictureBox1.BackColor);
            selectedIndex = -1;
            objects.Clear();
            inputPoints.Clear();
            render();
        }

        // при изменении размера необходимо обновить размер поля рисования и перерисовать
        private void Form1_SizeChanged(object sender, EventArgs e)
        {
            if (pictureBox1.Height > 0)
            {
                myBitmap = new Bitmap(pictureBox1.Width, pictureBox1.Height);
                g = Graphics.FromImage(myBitmap);
                g.SmoothingMode = SmoothingMode.AntiAlias;
                render();
            }
        }

        //перерисовка всех примитивов
        void render()
        {
            g.Clear(pictureBox1.BackColor);
            renderInput();

            int i = 0;
            if (!checkBox1.Checked)
                foreach (UniversalObject obj in objects)
                {
                    obj.render(g, pen);
                }
            else
                foreach (UniversalObject obj in objects)
                {
                    obj.render(g, pen, "" + i);
                    i++;
                }
            pictureBox1.Refresh();
        }

        // перерисовка режима рисования
        void renderInput()
        {
            if (inputPoints.Count > 0)
            {
                if (inputPoints.Count == 1)
                    g.DrawRectangle(pen, inputPoints[0].X, inputPoints[0].Y, 1, 1);
                for (int i = 1; i < inputPoints.Count; i++)
                {
                    g.DrawLine(pen, inputPoints[i - 1], inputPoints[i]);
                }
               
            }


        }

        // удаление
        private void button4_Click(object sender, EventArgs e)
        {
            if (selectedIndex != -1)
            {
                objects.RemoveAt(selectedIndex);
                selectedIndex = -1;
                render();
            }
        }
        //добавление примитива
        private void addPrimitive(int x, int y)
        {
            List<PointF> points = new List<PointF>();
            int mode = 0;
            switch (comboBox1.SelectedIndex)
            {
                default:
                    //фигура 1
                    points.Add(new PointF(x, y - 100));
                    points.Add(new PointF(x + 25, y - 50));
                    points.Add(new PointF(x + 50, y - 50));
                    points.Add(new PointF(x + 50, y - 50));
                    points.Add(new PointF(x + 50, y + 50));
                    points.Add(new PointF(x - 50, y + 50));
                    points.Add(new PointF(x - 50, y - 50));
                    points.Add(new PointF(x - 25, y - 50));

                    break;
                case 1:
                    //стрелка 3
                    int swift = 50;
                    points.Add(new PointF(x + 50, y + 50));
                    points.Add(new PointF(x + 50 + swift, y));
                    points.Add(new PointF(x + 50, y - 50));
                    points.Add(new PointF(x - 50, y - 50));
                    points.Add(new PointF(x - 50 + swift, y));
                    points.Add(new PointF(x - 50, y + 50));
                    break;
            }
            Primitive p = new Primitive(points, getColor());
            p.setMode(mode);
            objects.Add(p);
            render();
            updateBoxes();
        }

        //новая пара для ТМО
        private void button2_Click_1(object sender, EventArgs e)
        {
            int indexF = (int)comboBox2.SelectedIndex;
            int indexS = (int)comboBox3.SelectedIndex;

            int type = comboBox4.SelectedIndex;

            Primitive first;
            Primitive second;
            if (indexS != -1 && indexF != -1)
            {
                if (objects[indexF] is Primitive && objects[indexS] is Primitive)
                {
                    first = (Primitive)objects[indexF];
                    second = (Primitive)objects[indexS];
                }
                else
                {
                    MessageBox.Show("Один из объектов является сложным", "Ошибка ТМО", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }


            }
            else
            {
                MessageBox.Show("Не выбраны объекты для операции", "Ошибка ТМО", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!isCorrectForTMO(first) || !isCorrectForTMO(second))
            {
                MessageBox.Show("Прямая или кривая не могут быть выбраны для операции", "Ошибка ТМО", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (indexF != indexS)
            {
                selectedIndex = -1;

                objects.Add(new Pair(first, second, type));
                objects.Remove(first);
                objects.Remove(second);

                render();
                updateBoxes();
            }
            else
            {
                MessageBox.Show("Операция над одним объектом не возможна", "Ошибка ТМО", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        bool isCorrectForTMO(UniversalObject obj)
        {
            if (obj is Primitive)
            {
                int mode = ((Primitive)obj).getMode();
                if (mode == 1 || mode == 2) return false;
            }

            return true;
        }

        //после изменения количества фигур необходимо обновить списки для ТМО
        private void updateBoxes()
        {
            comboBox2.Items.Clear();
            comboBox2.Text = "Не выбран";
            for (int i = 0; i < objects.Count; i++)
            {
                comboBox2.Items.Add(i);
            }

            comboBox3.Items.Clear();
            comboBox3.Text = "Не выбран";
            for (int i = 0; i < objects.Count; i++)
            {
                comboBox3.Items.Add(i);
            }
        }


        /*  0 - Рисование примитивов
            1 - Работа с объектами
            2 - Рисование кривой
            3 - Добавление объектов*/
        private int getMode()
        {
            return comboBox7.SelectedIndex;
        }

        // при нажатии отражения
        private void button1_Click_1(object sender, EventArgs e)
        {
            if (getMode() == 1 && selectedIndex > -1)
            {
                UniversalObject obj = objects[selectedIndex];
                obj.mirror(obj.getCenter());
                render();
            }
            else {
                MessageBox.Show("Объект для преобразования не выбран", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void comboBox7_SelectedIndexChanged(object sender, EventArgs e)
        {
            

        }

        private void groupBox3_Enter(object sender, EventArgs e)
        {

        }
    }
}
