﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Presets
{
    [System.Serializable]
    public class EditDieAssignment
        : EditObject
    {
        public Dice.EditDie die;
        public Behaviors.EditBehavior behavior;
        public int defaultDieAssignmentIndex; // Used during auto-assign of user dice
    }

    class EditDieAssignmentConverter
        : JsonConverter<EditDieAssignment>
    {
        AppDataSet dataSet;
        public EditDieAssignmentConverter(AppDataSet dataSet)
        {
            this.dataSet = dataSet;
        }

        public override void WriteJson(JsonWriter writer, EditDieAssignment value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("deviceId");
            if (value.die != null)
                serializer.Serialize(writer, value.die.deviceId);
            else
                serializer.Serialize(writer, (ulong)0);
            writer.WritePropertyName("behaviorIndex");
            serializer.Serialize(writer, dataSet.behaviors.IndexOf(value.behavior));
            writer.WritePropertyName("defaultDieAssignmentIndex");
            serializer.Serialize(writer, value.defaultDieAssignmentIndex);
            writer.WriteEndObject();
        }

        public override EditDieAssignment ReadJson(JsonReader reader, System.Type objectType, EditDieAssignment existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (hasExistingValue)
                throw new System.NotImplementedException();

            var ret = new EditDieAssignment();    
            JObject jsonObject = JObject.Load(reader);
            System.UInt64 deviceId = jsonObject["deviceId"].ToObject<System.UInt64>();
            ret.die = dataSet.dice.Find(d => d.deviceId == deviceId);
            int behaviorIndex = jsonObject["behaviorIndex"].Value<int>();
            if (behaviorIndex >= 0 && behaviorIndex < dataSet.behaviors.Count)
                ret.behavior = dataSet.behaviors[behaviorIndex];
            else
                ret.behavior = null;
            ret.defaultDieAssignmentIndex = jsonObject["defaultDieAssignmentIndex"].Value<int>();
            return ret;
        }
    }

    [System.Serializable]
    public class EditPreset
        : EditObject
    {
        public string name;
        public string description;
        public List<EditDieAssignment> dieAssignments = new List<EditDieAssignment>();

        public bool CheckDependency(Dice.EditDie die)
        {
            return dieAssignments.Any(ass => ass.die == die);
        }

        public EditPreset Duplicate()
        {
            var ret = new EditPreset();
            ret.name = name;
            ret.description = description;
            ret.dieAssignments = new List<EditDieAssignment>();
            foreach (var a in dieAssignments)
            {
                var behaviorCopy = AppDataSet.Instance.DuplicateBehavior(a.behavior);
                ret.dieAssignments.Add(new EditDieAssignment() { die = a.die, behavior = behaviorCopy, defaultDieAssignmentIndex = 0 });
            }
            return ret;
        }

        public void DeleteBehavior(Behaviors.EditBehavior behavior)
        {
            foreach (var ass in dieAssignments)
            {
                if (ass.behavior == behavior)
                {
                    ass.behavior = null;
                }
            }
        }

        public bool DependsOnBehavior(Behaviors.EditBehavior behavior)
        {
            return dieAssignments.Any(d => d.behavior == behavior);
        }

        public void DeleteDie(Dice.EditDie die)
        {
            foreach (var ass in dieAssignments)
            {
                if (ass.die == die)
                {
                    ass.die = null;
                }
            }
        }

        public bool DependsOnDie(Dice.EditDie die)
        {
            return dieAssignments.Any(d => d.die == die);
        }

        public bool IsActive()
        {
            bool ret = true;
            foreach (var assignment in dieAssignments)
            {
                if (assignment.die == null)
                {
                    ret = false;
                }
                else if (assignment.die.currentBehavior != assignment.behavior)
                {
                    ret = false;
                    break;
                }
            }
            return ret;
        }
    }
}

