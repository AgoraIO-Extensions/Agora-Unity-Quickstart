using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;
using Agora.Rtm;
namespace io.agora.rtm.demo
{
    public class ListItemStateItem : ListItem<StateItem>
    {
        public InputField KeyInput;
        public InputField ValueInput;

        public override StateItem GetDataSource()
        {
            return new StateItem(this.KeyInput.text, this.ValueInput.text);
        }

        public override string ToString()
        {
            return string.Format("key:{0}, value:{1}", this.KeyInput.text, this.ValueInput.text);
        }

        public override void SetDataSource(StateItem dataSource)
        {
            this.KeyInput.text = dataSource.key;
            this.ValueInput.text = dataSource.value;
        }
    }
}
