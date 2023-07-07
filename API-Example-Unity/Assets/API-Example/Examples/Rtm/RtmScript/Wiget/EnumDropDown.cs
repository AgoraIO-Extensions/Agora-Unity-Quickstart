using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace io.agora.rtm.demo
{
    public class EnumDropDown : MonoBehaviour
    {
        private System.Array _enumValues;
        private int _selectValue;

        public void Init<T>()
        {
            _enumValues = System.Enum.GetValues(typeof(T));
            Dropdown dropdown = this.GetComponent<Dropdown>();
            List<Dropdown.OptionData> optionDatas = new List<Dropdown.OptionData>();
            foreach (var vaule in _enumValues)
            {
                optionDatas.Add(new Dropdown.OptionData(vaule.ToString()));
            }
            dropdown.ClearOptions();
            dropdown.AddOptions(optionDatas);
            dropdown.onValueChanged.AddListener(this.OnValueChanged);
            _selectValue = (int)_enumValues.GetValue(0);
        }

        public void OnValueChanged(int args)
        {
            _selectValue = (int)_enumValues.GetValue(args);
        }

        public int GetSelectValue()
        {
            Debug.Log("GetSeletValue: " + _selectValue);
            return _selectValue;
        }





    }
}
