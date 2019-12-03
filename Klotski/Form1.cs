using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Klotski
{
    public partial class Form1 : Form
    {
        //声明绘图工具
        Graphics g = null;
        Pen pen = null;
        Brush brush = null;

        //8个可移动的华容道柱体
        Stick[] sticks = new Stick[8];

        //用位图表示华容道
        //0 空地；1 墙体；2 华容道柱体；9 目标对接的两个华容道柱体；
        int[,] bitMap = new int[15, 12]
        {
            {1,1,1,1,1,1,1,1,1,1,1,1},
            {1,1,1,1,1,9,0,0,0,0,0,1},
            {1,1,1,1,1,9,0,0,0,0,0,1},
            {1,1,1,1,1,0,2,2,2,0,0,1},
            {1,1,1,1,1,0,0,0,0,1,1,1},
            {1,0,0,0,0,0,0,1,2,0,0,1},
            {1,0,0,0,2,0,0,1,2,0,0,1},
            {1,0,0,0,2,0,0,0,0,0,0,1},
            {1,2,0,0,2,0,2,2,2,2,2,1},
            {1,2,0,0,0,0,0,0,2,0,0,1},
            {1,0,0,0,0,0,0,0,2,0,0,1},
             {1,2,2,2,2,2,0,0,2,0,0,1},
            {1,1,1,1,0,9,0,0,0,0,0,1},
            {1,1,1,1,0,9,0,0,1,1,1,1},
            {1,1,1,1,1,1,1,1,1,1,1,1}
        };

        //用以临时记录激活stick时的鼠标坐标
        Point tPoint = new Point();

        //记录游内部窗口左上角的坐标
        int startX = 90;
        int startY = 75;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //设置双缓冲
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.DoubleBuffer, true);

            //载入华容道的信息
            sticks[0] = new Stick(420, 195, 60, 30, 0, false, 13, 6, 2);
            sticks[1] = new Stick(300, 75, 60, 30, 0, false, 9, 2, 2);
            sticks[2] = new Stick(240, 165, 90, 30, 0, false, 7, 5, 3);
            sticks[3] = new Stick(330, 285, 90, 30, 0, false, 10, 9, 3);
            sticks[4] = new Stick(210, 285, 60, 30, 0, false, 6, 9, 2);
            sticks[5] = new Stick(300, 225, 30, 150, 1, false, 9, 7, 5);
            sticks[6] = new Stick(390, 75, 30, 150, 1, false, 12, 2, 5);
            sticks[7] = new Stick(150, 225, 30, 90, 1, false, 4, 7, 3);
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            //初始化绘图工具
            g = e.Graphics;

            DrawKlotski(g);
        }

        public void DrawKlotski(Graphics g)
        {
            //绘制外框
            pen = new Pen(Color.Black);
            g.DrawRectangle(pen, 90, 75, 390, 300);

            //绘制目标对接的两个华容道柱体
            brush = new SolidBrush(Color.RosyBrown);
            g.FillRectangle(brush, 90, 195, 60, 30);
            g.FillRectangle(brush, sticks[0].x, sticks[0].y, sticks[0].width, sticks[0].height);

            //绘制墙体
            brush = new SolidBrush(Color.Gray);
            g.FillRectangle(brush, 90, 75, 120, 120);
            g.FillRectangle(brush, 180, 315, 30, 60);
            g.FillRectangle(brush, 420, 75, 60, 90);
            g.FillRectangle(brush, 450, 285, 30, 90);
            g.FillRectangle(brush, 210, 255, 60, 30);

            //绘制可移动的华容道柱体
            brush = new SolidBrush(Color.LightSeaGreen);
            for (int i = 1; i < sticks.Length; i++)
            {
                g.FillRectangle(brush, sticks[i].x, sticks[i].y, sticks[i].width, sticks[i].height);
            }
        }

        //判定鼠标是否在任意的华容道柱体(sticks)内
        public bool isInStickX(Stick stick, Point point)
        {
            if ((point.x >= stick.x) && (point.x <= (stick.x + stick.width)) && (point.y >= stick.y) && (point.y <= (stick.y + stick.height)))
            {
                return true;
            }
            return false;
        }

        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            //鼠标当前坐标与激活stick时的临时坐标的差
            int differenceOfX = 0;
            int differenceOfY = 0;

            //若有stick被选中
            if (sticks[0].isActived || sticks[1].isActived || sticks[2].isActived || sticks[3].isActived || sticks[4].isActived || sticks[5].isActived || sticks[6].isActived || sticks[7].isActived)
            {
                differenceOfX = e.Location.X - tPoint.x;
                differenceOfY = e.Location.Y - tPoint.y;

                for (int i = 0; i < sticks.Length; i++)
                {
                    //如果当前stick被选中且为横向移动的stick
                    if (sticks[i].isActived && sticks[i].axis == 0)
                    {
                        if (differenceOfX >= 0)     //向右移动
                        {
                            //如果移动正方向的为空地
                            if ((bitMap[sticks[i].i + sticks[i].length - 1, sticks[i].j - 1] == 0))
                            {
                                sticks[i].x += 1;
                                //如果走完了一格子，将前进方向的下一个格子的位图信息设置为2，stick左端的格子设置为0，最后，修改位图的映射
                                if ((sticks[i].x + 30 * sticks[i].length) == (startX + 30 * (sticks[i].i + sticks[i].length - 1)))
                                {
                                    bitMap[sticks[i].i + sticks[i].length - 1, sticks[i].j - 1] = 2;
                                    bitMap[sticks[i].i - 1, sticks[i].j - 1] = 0;
                                    sticks[i].i += 1;
                                }
                            }
                        }
                        else if (differenceOfX < 0)      //向左移动
                        {
                            //如果移动负方向的为空地
                            if ((bitMap[sticks[i].i - 2, sticks[i].j - 1] == 0))
                            {
                                sticks[i].x -= 1;
                                //如果走完了一格子，将前进方向的下一个格子的位图信息设置为2，stick右端的格子设置为0，最后，修改位图的映射
                                if (sticks[i].x == (startX + 30 * (sticks[i].i - 3)))
                                {
                                    bitMap[sticks[i].i - 2, sticks[i].j - 1] = 2;
                                    bitMap[sticks[i].i + sticks[i].length - 2, sticks[i].j - 1] = 0;
                                    sticks[i].i -= 1;
                                }
                            }
                        }
                        Invalidate();
                        break;
                    }
                    //如果当前stick被选中且为竖向移动的stick
                    else if (sticks[i].isActived && sticks[i].axis == 1)
                    {
                        if (differenceOfY >= 0)     //向下移动
                        {
                            //如果移动方向为空地
                            if (bitMap[sticks[i].i - 1, sticks[i].j + sticks[i].length - 1] == 0)
                            {
                                sticks[i].y += 1;
                                //如果走完了一格子，将前进方向的下一个格子的位图信息设置为2，stick上端的格子设置为0，最后，修改位图的映射
                                if ((sticks[i].y + 30 * sticks[i].length) == (startY + 30 * (sticks[i].j + sticks[i].length - 1)))
                                {
                                    bitMap[sticks[i].i - 1, sticks[i].j - 1] = 0;
                                    bitMap[sticks[i].i - 1, sticks[i].j + sticks[i].length - 1] = 2;
                                    sticks[i].j += 1;
                                }
                            }
                        }
                        else if (differenceOfY < 0)      //向上移动
                        {
                            //如果移动方向为空地
                            if ((bitMap[sticks[i].i - 1, sticks[i].j - 2] == 0))
                            {
                                sticks[i].y -= 1;
                                //如果走完了一格子，将前进方向的下一个格子的位图信息设置为2，stick下端的格子设置为0，最后，修改位图的映射
                                if (sticks[i].y == (startY + 30 * (sticks[i].j - 3)))
                                {
                                    bitMap[sticks[i].i - 1, sticks[i].j - 2] = 2;
                                    bitMap[sticks[i].i - 1, sticks[i].j + sticks[i].length - 2] = 0;
                                    sticks[i].j -= 1;
                                }
                            }
                        }
                        Invalidate();
                        break;
                    }
                }
            }
        }

        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            tPoint.x = e.Location.X;
            tPoint.y = e.Location.Y;

            for (int i = 0; i < sticks.Length; i++)
            {
                //如果华容道柱体(stick)未被选中，且鼠标点击的位置在华容道柱体内（sticks），该华容道柱体stick状态设置为被选中
                if (!sticks[i].isActived && (isInStickX(sticks[i], tPoint)))
                {
                    sticks[i].isActived = true;
                    break;
                }
            }
        }

        private void Form1_MouseUp(object sender, MouseEventArgs e)
        {
            //判定胜利，按下确定后，关闭游戏
            if (sticks[0].x == (startX + (2) * 30))
            {
                MessageBox.Show("You won!");
                this.Close();
            }
            for (int i = 0; i < sticks.Length; i++)
            {
                //当鼠标松开的时候，被选中的华容道柱体(sticks)的状态设置为未被选中
                if (sticks[i].isActived)
                {
                    sticks[i].isActived = false;
                    break;
                }
            }
        }
    }

    public class Stick      //华容道柱体
    {
        public int x;       //柱体的x
        public int y;       //柱体的y
        public int width;         //柱体的宽
        public int height;      //柱体的高
        public int axis;        // 横向/竖向
        public bool isActived;      //是否被选中
        public int i;       //到位图的x映射
        public int j;       //到位图的y映射
        public int length;      //柱体的长度由length个方块组成

        public Stick(int x, int y, int width, int height, int axis, bool isActived, int i, int j, int length)
        {
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
            this.axis = axis;
            this.isActived = isActived;
            this.i = i;
            this.j = j;
            this.length = length;
        }
    }

    public class Point
    {
        public int x;
        public int y;
    }
}
