using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LEV_ClickSciptExtended
{
    public class LEV_ClickScriptClaimRect
    {
        public Rect Area { get; private set; }
        public RectTransform RTransform { get; private set; }
        public bool Active { get; private set; }
        public bool Enabled { get; private set; }

        public LEV_ClickScriptClaimRect(Rect area)
        {
            Area = area;
        }

        ~LEV_ClickScriptClaimRect()
        {
            LEV_ClickScriptX.RemoveClaim(this);
        }

        // Method to update the Rect
        public void SetArea(Rect newArea)
        {
            Area = newArea;
        }

        public void SetActive(bool state)
        {
            Active = state;
            SetVisibility();
        }
        public void SetEnabled(bool state)
        {
            Enabled = state;
            SetVisibility();
        }

        public void SetRectTransform(RectTransform rectTransform)
        {
            RTransform = rectTransform;
        }

        private void SetVisibility()
        {
            if (RTransform != null)
            {
                if (Enabled && Active)
                {
                    RTransform.gameObject.SetActive(true);
                }
                else
                {
                    RTransform.gameObject.SetActive(false);
                }
            }
        }        
    }
}
