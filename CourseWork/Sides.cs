using System;
using System.Collections.Generic;
using System.Text;

namespace GSCEditor
{
    class Sides
    {
        // списки левых и правых границ, должны быть равны размером
        private List<float> left;
        private List<float> right;

        public Sides(List<float> left, List<float> right) {
            this.left = left;
            this.right = right;
        }

        public List<float> getLeft() {
            return left;
        }

        public List<float> getRight()
        {
            return right;
        }
    }
}
