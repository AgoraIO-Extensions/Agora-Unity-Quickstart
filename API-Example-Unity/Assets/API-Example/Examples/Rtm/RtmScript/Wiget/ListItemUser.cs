using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace io.agora.rtm.demo
{
    public class ListItemUser : ListItem<string>
    {
        public InputField UserInput;

        public override string GetDataSource()
        {
            return this.UserInput.text;
        }

        public override string ToString()
        {
            return this.UserInput.text;
        }

        public override void SetDataSource(string dataSource)
        {
            this.UserInput.text = dataSource;
        }
    }
}
