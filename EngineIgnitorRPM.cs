using System;
using System.Text;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using JSI;
using EngineIgnitor;
using UnityEngine;

namespace EngineIgnitorRPM
{
	public class EngineIgnitorRPM : InternalModule
	{
		public class EngineInformation
		{
			static Color32 warningColor = new Color32(255, 0, 0, 255);
			static Color32 selectedColor = new Color32(0, 255, 0, 255);

			static Color32 redColor = new Color32(255, 0, 0, 255);
			static Color32 orangeColor = new Color32(255, 128, 0, 255);
			static Color32 yellowColor = new Color32(255, 255, 0, 255);
			static Color32 kellyColor = new Color32(128, 255, 0, 255);
			static Color32 greenColor = new Color32(0, 255, 0, 255);
			static Color32 cyanColor = new Color32(0, 255, 255, 255);
			static Color32 blueColor = new Color32(0, 0, 255, 255);
			static Color32 purpleColor = new Color32(255, 0, 255, 255);
			static Color32 greyColor = new Color32(128, 128, 128, 255);
			static Color32 whiteColor = new Color32(255, 255, 255, 255);

			public EngineWrapper engine;
			public ModuleEngineIgnitor ignitor;
			public bool selected;

			public EngineInformation(ModuleEngines engine)
			{
				this.engine = new EngineWrapper(engine);
				if (engine.part.Modules.Contains("ModuleEngineIgnitor"))
					this.ignitor = engine.part.Modules["ModuleEngineIgnitor"] as ModuleEngineIgnitor;
				else
					this.ignitor = null;
				selected = false;
			}
			public EngineInformation(ModuleEnginesFX engine)
			{
				this.engine = new EngineWrapper(engine);
				if (engine.part.Modules.Contains("ModuleEngineIgnitor"))
					this.ignitor = engine.part.Modules["ModuleEngineIgnitor"] as ModuleEngineIgnitor;
				else
					this.ignitor = null;
				selected = false;
			}

			public TextMenu.Item itemTitle = null;
			public TextMenu.Item itemEIInfo = null;

			public void UpdateListItem()
			{
				itemTitle.isSelected = selected;
				string partTitle = engine.part.partInfo.title;
				if (partTitle.Length > 35) partTitle = partTitle.Substring(0, 32) + "...";
				itemTitle.labelText = (selected ? "*" : " ") + partTitle;
				if (ignitor != null)
				{
					itemEIInfo.labelText = "  " + (IsEngineActivated() ? JUtil.ColorToColorTag(whiteColor) + "ON  " : JUtil.ColorToColorTag(greyColor) + "OFF ")
						+ GetColoredEngineState(GetValueByReflection<ModuleEngineIgnitor.EngineIgnitionState>(ignitor, "engineState", true)) + " "
						+ GetColoredFuelFlow(GetValueByReflection<string>(ignitor, "ullageState", true)) + " "
						+ GetColoredIgnitorState(ignitor);
					Debug.Log(itemEIInfo.labelText);
				}
				else
				{
					itemEIInfo.labelText = (IsEngineActivated() ? JUtil.ColorToColorTag(whiteColor) + "  ON" : JUtil.ColorToColorTag(greyColor) + "  OFF"); 
				}
			}

			private string GetColoredEngineState(ModuleEngineIgnitor.EngineIgnitionState state)
			{
				switch (state)
				{
					case ModuleEngineIgnitor.EngineIgnitionState.NOT_IGNITED:
						return JUtil.ColorToColorTag(greyColor) + "NOIGN";
					case ModuleEngineIgnitor.EngineIgnitionState.HIGH_TEMP:
						return JUtil.ColorToColorTag(whiteColor) + "HITMP";
					case ModuleEngineIgnitor.EngineIgnitionState.IGNITED:
						return JUtil.ColorToColorTag(yellowColor) + "IGNTD";
					default:
						return "     ";
				}
			}

			private string GetColoredFuelFlow(string fuelFlow)
			{
				switch (fuelFlow)
				{
					case "Pressurized":
						return JUtil.ColorToColorTag(greenColor) + "PRESS+";
					case "Unpressurized":
						return JUtil.ColorToColorTag(redColor) + "PRESS-";
					case "Very Stable":
						return JUtil.ColorToColorTag(greenColor) + "STBLE+";
					case "Stable":
						return JUtil.ColorToColorTag(kellyColor) + "STBLE-";
					case "Risky":
						return JUtil.ColorToColorTag(yellowColor) + "RISKY+";
					case "Very Risky":
						return JUtil.ColorToColorTag(orangeColor) + "RISKY-";
					case "Unstable":
						return JUtil.ColorToColorTag(redColor) + "UNSTB+";
					case "Very Unstable":
						return JUtil.ColorToColorTag(redColor) + "UNSTB-";
					default:
						return "      ";
				}
			}

			private string GetColoredIgnitorState(ModuleEngineIgnitor ignitor)
			{
				string result = JUtil.ColorToColorTag(whiteColor) + "IGNT:";
				int ignitions = ignitor.ignitionsRemained;
				if (ignitor.ignitorResources.Count != 0)
				{
					int minTimes = int.MaxValue;
					foreach (IgnitorResource ir in ignitor.ignitorResources)
					{
						List<PartResource> sources = new List<PartResource>();
						ignitor.part.GetConnectedResources(ir.name.GetHashCode(), PartResourceLibrary.Instance.resourceDefinitions[ir.name].resourceFlowMode, sources);
						double totalAmount = 0.0;
						foreach (PartResource pr in sources)
						{
							totalAmount += pr.amount;
						}
						int times = (int)(totalAmount / ir.amount);
						if (minTimes > times) minTimes = times;
					};
					ignitions = Math.Min(ignitions, minTimes);
				}
				result += (ignitions == 0 ? JUtil.ColorToColorTag(redColor) : (ignitions == 1 ? JUtil.ColorToColorTag(orangeColor) : (ignitions == 2 ? JUtil.ColorToColorTag(yellowColor) : JUtil.ColorToColorTag(greenColor)))) + ignitions.ToString();
				return result;
			}

			public string ShowInfo(int width, int height)
			{
				string result = "";

				string partTitle = engine.part.partInfo.title;
				if (partTitle.Length > 36) partTitle = partTitle.Substring(0, 33) + "..."; 
				
				if (ignitor == null)
				{
					result = (selected ? JUtil.ColorToColorTag(selectedColor) + "*" : " ") + partTitle + Environment.NewLine
					+ (IsEngineActivated() ? "  ON  " : "  OFF ") + Environment.NewLine + "  Engine ignitor not available.";
					for (int i = 0; i < height - 2; ++i)
						result += Environment.NewLine;

					return result;
				}

				int lines = 0;

				result = (selected ? JUtil.ColorToColorTag(selectedColor) + "*" : " ") + partTitle + Environment.NewLine
					+ (IsEngineActivated() ? "  ON  " : "  OFF ")
					+ "  State: " + GetValueByReflection<ModuleEngineIgnitor.EngineIgnitionState>(ignitor, "engineState", true).ToString() + Environment.NewLine
					+ "  Fuel Flow: " + GetValueByReflection<string>(ignitor, "ullageState", true) + Environment.NewLine
					+ "  Auto-Ignite: " + GetValueByReflection<string>(ignitor, "autoIgnitionState", true) + Environment.NewLine
					+ "  Ignitor: " + GetValueByReflection<string>(ignitor, "ignitionsAvailableString", true) + Environment.NewLine;
				lines += 5;
				if (ignitor.ignitorResources.Count != 0)
				{
					result += "  Ignitor Resource" + (ignitor.ignitorResources.Count == 1 ? "" : "s") + ":" + Environment.NewLine;
					foreach (IgnitorResource ir in ignitor.ignitorResources)
					{
						List<PartResource> sources = new List<PartResource>();
						ignitor.part.GetConnectedResources(ir.name.GetHashCode(), PartResourceLibrary.Instance.resourceDefinitions[ir.name].resourceFlowMode, sources);
						double totalAmount = 0.0;
						foreach (PartResource pr in sources)
						{
							totalAmount += pr.amount;
						}
						result += "   " + ir.name + ": " + totalAmount.ToString("F3") + "/" + ir.amount.ToString("F3") + (totalAmount < ir.amount * 1.0 ? " " + JUtil.ColorToColorTag(redColor) + "OUT" : (totalAmount < ir.amount * 2.0 ? " " + JUtil.ColorToColorTag(redColor) + "LOW" : "")) + Environment.NewLine;
					};
					lines += 1 + ignitor.ignitorResources.Count;
				}

				for (int i = 0; i < (height - lines); ++i)
					result += Environment.NewLine;

				return result;
			}

			public bool IsEngineActivated()
			{
				foreach (BaseEvent baseEvent in engine.Events)
				{
					//Debug.Log("Engine's event: " + baseEvent.name);
					if (baseEvent.name.IndexOf("activate", StringComparison.CurrentCultureIgnoreCase) >= 0)
					{
						//Debug.Log("IsEngineActivated: " + baseEvent.name + " " + baseEvent.active.ToString() + " " + baseEvent.guiActive.ToString());
						if (baseEvent.active == false)
							return true;
					}
				}

				return false;
			}

			public void ActivateEngine()
			{
				if (IsEngineActivated() == false)
				{
					foreach (BaseEvent baseEvent in engine.Events)
					{
						//Debug.Log("Engine's event: " + baseEvent.name);
						if (baseEvent.name.IndexOf("activate", StringComparison.CurrentCultureIgnoreCase) >= 0)
						{
							baseEvent.Invoke();
						}
					}
				}
			}

			public void ShutdownEngine()
			{
				if (IsEngineActivated() == true)
				{
					engine.SetRunningGroupsActive(false);
					foreach (BaseEvent baseEvent in engine.Events)
					{
						//Debug.Log("Engine's event: " + baseEvent.name);
						if (baseEvent.name.IndexOf("shutdown", StringComparison.CurrentCultureIgnoreCase) >= 0)
						{
							baseEvent.Invoke();
						}
					}
					engine.SetRunningGroupsActive(false);
				}
			}

			private T GetValueByReflection<T>(object obj, string fieldName, bool isPrivate)
			{
				T result = default(T);
				try
				{
					if (isPrivate)
						result = (T)(obj.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(obj));
					else
						result = (T)(obj.GetType().GetField(fieldName).GetValue(obj));
				}
				catch (Exception e)
				{}
				return result;
			}
		}

		// KSPFields start here.
		[KSPField]
		public int buttonUp = 0;
		[KSPField]
		public int buttonDown = 1;
		[KSPField]
		public int buttonEnter = 2;
		//[KSPField]
		//public int buttonEsc = 3;
		//[KSPField]
		//public int buttonHome = 4;
		[KSPField]
		public int button8 = 5;
		[KSPField]
		public int button9 = 6;
		[KSPField]
		public int button10 = 7;
		[KSPField]
		public int button6 = 8;
		[KSPField]
		public int button7 = 9;

		[KSPField]
		public string itemColor = string.Empty;
		private Color itemColorValue = Color.white;
		[KSPField]
		public string selectedColor = string.Empty;
		private Color selectedColorValue = Color.green;
		[KSPField]
		public string unavailableColor = string.Empty;
		private Color unavailableColorValue = Color.gray;
		[KSPField]
		public string titleColor = string.Empty;
		private Color titleColorValue = Color.cyan;
		[KSPField]
		public string sepColor = string.Empty;
		private Color sepColorValue = Color.yellow;

		private bool detailMode = false;
		private bool pageActiveState;

		// KSPFields end here.

		private List<EngineInformation> engines = new List<EngineInformation>();
		
		private TextMenu engineListMenu = new TextMenu();

		public void PageActive(bool active, int pageNumber)
		{
			pageActiveState = active;
		}

		public string ShowMenu(int width, int height)
		{
			UpdateEngineReferences();

			var result = new StringBuilder();

			result.Append(titleColor).Append("Engine Ignitor MFD").Append(itemColor).AppendLine(String.Format("{0}/{1}", (engines.Count == 0 ? 0 :engineListMenu.currentSelection/2 + 1), engines.Count).PadLeft(40 - 18));
			result.AppendLine(sepColor + new String('=', 40));
			height-=4;

			if (detailMode == false)
			{
				if (engineListMenu.currentSelection >= engineListMenu.Count)
					engineListMenu.currentSelection = engineListMenu.Count - 1;
				if (engineListMenu.currentSelection % 2 == 1)
					engineListMenu.currentSelection -= 1;
				string menuStr = engineListMenu.ShowMenu(width + 66, height);
				result.Append(menuStr);
				int index = 0;
				int lines = 0;
				index = menuStr.IndexOf(Environment.NewLine, index);
				while (index >= 0)
				{
					lines++;
					index = menuStr.IndexOf(Environment.NewLine, index + 1);
				};
				lines = height - lines;
				for(int i = 0; i < lines; ++i)
					result.AppendLine();
				result.AppendLine(sepColor + new String('-', 40));
				result.AppendLine("[@x-6]  ACTV  SHUT TOGL UNSEL INFO");
			}
			else
			{
				if (engines.Count == 0)
				{
					for (int i = 0; i < height; ++i)
						result.AppendLine();
				}
				else
				{
					result.Append(engines[engineListMenu.currentSelection / 2].ShowInfo(width, height));
				}
				result.AppendLine(sepColor + new String('-', 40));
				result.AppendLine("[@x-6]                        " + engineListMenu.selectedColor + "INFO");
			}

			return result.ToString();
		}

		public void ClickProcessor(int buttonID)
		{
			if (buttonID == buttonUp)
			{
				engineListMenu.PreviousItem();
				engineListMenu.PreviousItem();
				if (engineListMenu.currentSelection % 2 == 1)
					engineListMenu.PreviousItem();
			}
			if (buttonID == buttonDown)
			{
				engineListMenu.NextItem();
				engineListMenu.NextItem();
				if(engineListMenu.currentSelection % 2 == 1)
					engineListMenu.PreviousItem();
			}
			if (buttonID == buttonEnter)
			{
				foreach (EngineInformation ei in engines)
				{
					if (ei.itemTitle == engineListMenu.GetCurrentItem())
					{
						ei.selected = !ei.selected;
						ei.UpdateListItem();
						break;
					}
				}
			}
			if (buttonID == button6)
			{
				if (detailMode) return;
				foreach (EngineInformation ei in engines)
				{
					if (ei.selected)
					{
						ei.ActivateEngine();
						ei.UpdateListItem();
					}
				}
			}
			if (buttonID == button7)
			{
				if (detailMode) return;
				foreach (EngineInformation ei in engines)
				{
					if (ei.selected)
					{
						ei.ShutdownEngine();
						ei.UpdateListItem();
					}
				}
			}
			if (buttonID == button8)
			{
				if (detailMode) return;
				foreach (EngineInformation ei in engines)
				{
					if (ei.selected)
					{
						if(ei.IsEngineActivated())
							ei.ShutdownEngine();
						else
							ei.ActivateEngine();
						ei.UpdateListItem();
					}
				}
			}
			if (buttonID == button9)
			{
				if (detailMode) return;
				foreach (EngineInformation ei in engines)
				{
					if (ei.selected)
					{
						ei.selected = false;
						ei.UpdateListItem();
					}
				}
			}
			if (buttonID == button10)
			{
				detailMode = !detailMode;
			}
		}
		/* Note to self:
		foreach (ThatEnumType item in (ThatEnumType[]) Enum.GetValues(typeof(ThatEnumType)))
		can save a lot of time here.
		*/
		private void UpdateEngineReferences()
		{
			for(int i = 0; i < engines.Count; ++i)
			{
				if (engines[i].engine.part == null || engines[i].engine.vessel == null || engines[i].engine.vessel != vessel)
				{
					engineListMenu.Remove(engines[i].itemTitle);
					engineListMenu.Remove(engines[i].itemEIInfo);
					engines.RemoveAt(i);
					--i;
				}
			}
			foreach(Part part in vessel.Parts)
			{
				int hasEngine = 0;
				if(part.Modules.Contains("ModuleEngines"))
					hasEngine = 1;
				else if (part.Modules.Contains("ModuleEnginesFX"))
					hasEngine = 2;

				if(hasEngine != 0)
				{
					// Skip all these engines existed.
					bool alreadyExists = false;
					foreach (EngineInformation ei in engines)
					{
						if (ei.engine.part == part)
						{
							alreadyExists = true;
							break;
						}
					}
					if (alreadyExists) continue;
					
					EngineInformation newEngine = null;
					if(hasEngine == 1)
						newEngine = new EngineInformation(part.Modules["ModuleEngines"] as ModuleEngines);
					else
						newEngine = new EngineInformation(part.Modules["ModuleEnginesFX"] as ModuleEnginesFX);

					TextMenu.Item listItemTitle = new TextMenu.Item();
					newEngine.itemTitle = listItemTitle;
					TextMenu.Item listItemEIInfo = new TextMenu.Item();
					newEngine.itemEIInfo = listItemEIInfo;

					engines.Add(newEngine);

					engineListMenu.Add(listItemTitle);
					engineListMenu.Add(listItemEIInfo);
				}
			}
			foreach (EngineInformation ei in engines)
			{
				ei.UpdateListItem();
			}
		}

		public void Start()
		{

			// I guess I shouldn't have expected Squad to actually do something nice for a modder like that.
			// In 0.23, loading in non-alphabetical order is still broken.
			InstallationPathWarning.Warn("RPMEngineIgnitor");

			if (!string.IsNullOrEmpty(itemColor))
				itemColorValue = ConfigNode.ParseColor32(itemColor);
			else
				itemColor = JUtil.ColorToColorTag(itemColorValue);
			if (!string.IsNullOrEmpty(selectedColor))
				selectedColorValue = ConfigNode.ParseColor32(selectedColor);
			else
				selectedColor = JUtil.ColorToColorTag(selectedColorValue); 
			if (!string.IsNullOrEmpty(unavailableColor))
				unavailableColorValue = ConfigNode.ParseColor32(unavailableColor);
			else
				unavailableColor = JUtil.ColorToColorTag(unavailableColorValue); 
			if (!string.IsNullOrEmpty(titleColor))
				titleColorValue = ConfigNode.ParseColor32(titleColor);
			else
				titleColor = JUtil.ColorToColorTag(titleColorValue);
			if (!string.IsNullOrEmpty(sepColor))
				sepColorValue = ConfigNode.ParseColor32(sepColor);
			else
				sepColor = JUtil.ColorToColorTag(sepColorValue);

			UpdateEngineReferences();

			engineListMenu.labelColor = JUtil.ColorToColorTag(itemColorValue);
			engineListMenu.selectedColor = JUtil.ColorToColorTag(selectedColorValue);
			engineListMenu.disabledColor = JUtil.ColorToColorTag(unavailableColorValue);
			
		}
	}
}