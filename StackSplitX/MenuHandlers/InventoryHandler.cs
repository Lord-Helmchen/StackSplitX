﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using System.Diagnostics;

namespace StackSplitX.MenuHandlers
{
    public class InventoryHandler
    {
        /// <summary>If the handler has been initialized yet by calling Init.</summary>
        public bool Initialized => this.NativeInventory != null;

        /// <summary>Convenience for grabbing native inventory buttons.</summary>
        private List<ClickableComponent> Inventory => this.NativeInventory.inventory;

        /// <summary>Convenience for grabbing native inventory items.</summary>
        private List<Item> InventoryItems => this.NativeInventory.actualInventory;

        /// <summary>Native inventory.</summary>
        private InventoryMenu NativeInventory;

        /// <summary>Inventory interface bounds.</summary>
        private Rectangle Bounds;
        
        /// <summary>Where the user clicked so moving </summary>
        private Point SelectedItemPosition;
        
        /// <summary>The held item field owned by the parent menu that contains the inventory.</summary>
        private IPrivateField<Item> HeldItemField;

        /// <summary>The hovered item field owned by the parent menu that contains the inventory.</summary>
        private IPrivateField<Item> HoveredItemField;
        
        /// <summary>Currently hovered item in the inventory.</summary>
        private Item HoveredItem;

        private readonly IReflectionHelper Reflection;
        private readonly IMonitor Monitor;


        public InventoryHandler(IReflectionHelper reflection, IMonitor monitor)
        {
            this.Reflection = reflection;
            this.Monitor = monitor;
        }

        /// <summary>This must be called everytime the inventory is opened/resized.</summary>
        /// <param name="inventory">Native inventory.</param>
        public void Init(InventoryMenu inventory, IPrivateField<Item> heldItemField, IPrivateField<Item> hoveredItemField)
        {
            this.NativeInventory = inventory;
            this.HeldItemField = heldItemField;
            this.HoveredItemField = hoveredItemField;

            // Create the bounds around the inventory
            var first = this.Inventory[0].bounds;
            var last = this.Inventory.Last().bounds;
            this.Bounds = new Rectangle(
                first.X,
                first.Y, 
                last.X + last.Width - first.X, 
                last.Y + last.Height - first.Y);
        }

        /// <summary>Broad phase check to see if the inventory interface was clicked.</summary>
        /// <param name="mousePos">Mouse position.</param>
        public bool WasClicked(Point mousePos)
        {
            Debug.Assert(this.Initialized);
            return this.Bounds.Contains(mousePos);
        }

        /// <summary>Broad phase check to see if the inventory interface was clicked.</summary>
        /// <param name="mouseX">Mouse X position.</param>
        /// <param name="mouseY">Mouse Y position.</param>
        public bool WasClicked(int mouseX, int mouseY)
        {
            Debug.Assert(this.Initialized);
            return this.Bounds.Contains(mouseX, mouseY);
        }

        /// <summary>Stores the data needed to be able to split an item stack. This must be called before CanSplitSelectedItem and SplitSelectedItem.</summary>
        /// <param name="mouseX">Mouse x position.</param>
        /// <param name="mouseY">Mouse y position.</param>
        public void SelectItem(int mouseX, int mouseY)
        {
            Debug.Assert(this.Initialized);

            this.SelectedItemPosition = new Point(mouseX, mouseY);
            this.HoveredItem = this.HoveredItemField.GetValue();
        }

        /// <summary>Checks if the selected item can be split. SelectItem must be called first.</summary>
        public bool CanSplitSelectedItem()
        {
            Debug.Assert(this.Initialized);

            var hoveredItem = this.HoveredItem;
            var heldItem = this.HeldItemField.GetValue();

            return (hoveredItem != null && hoveredItem.Stack > 1 &&
                   (heldItem == null || (hoveredItem.canStackWith(heldItem) && heldItem.Stack < heldItem.maximumStackSize())));
        }

        /// <summary>Updates the stack values of the hovered and held item.</summary>
        /// <param name="stackAmount">The amount to be added to the held amount.</param>
        public void SplitSelectedItem(int stackAmount)
        {
            Debug.Assert(this.HeldItemField != null && this.HoveredItemField != null);

            var hoveredItem = this.HoveredItem;
            var heldItem = this.HeldItemField.GetValue();

            int hoveredItems = hoveredItem.Stack;
            int heldItems = (heldItem != null ? heldItem.Stack : 0);
            int totalItems = hoveredItems + heldItems;
            int maxStack = hoveredItem.maximumStackSize();

            // Run native click code to get the selected item
            heldItem = this.NativeInventory.rightClick(this.SelectedItemPosition.X, this.SelectedItemPosition.Y, heldItem);
            Debug.Assert(heldItem != null);

            // Clamp the amount to the total number of items
            stackAmount = (int)MathHelper.Clamp(stackAmount, 0, hoveredItems);
            heldItem.Stack = Math.Min(maxStack, heldItems + stackAmount);
            heldItem = heldItem.Stack > 0 ? heldItem : null;

            // If we couldn't grab all that we wanted then only subtract the amount we were able to grab
            int overflow = Math.Max((heldItems + stackAmount) - maxStack, 0);
            hoveredItem.Stack = hoveredItems - (stackAmount - overflow);

            // Remove the item from the inventory as it's now all being held.
            if (hoveredItem.Stack == 0)
                RemoveItemFromInventory(hoveredItem);

            // Update the native fields
            this.HeldItemField.SetValue(heldItem);
        }

        /// <summary>Runs the default shift+right-click behavior on the selected item.</summary>
        public void CancelSplit()
        {
            if (this.Initialized && this.HoveredItem != null)
            {
                // Split with the default amount to simulate the default behaviour
                SplitSelectedItem(GetDefaultSplitStackAmount());
            }
        }

        /// <summary>Gets the stack amount you would usually have when shift+right-clicking.</summary>
        public int GetDefaultSplitStackAmount()
        {
            return (int)Math.Ceiling(this.HoveredItem.Stack / 2.0);
        }

        /// <summary>Removes an item from the native inventory</summary>
        /// <param name="item">The item to remove.</param>
        private void RemoveItemFromInventory(Item item)
        {
            var index = this.InventoryItems.IndexOf(item);
            if (index >= 0 && index < this.InventoryItems.Count)
            {
                this.InventoryItems[index] = null;
            }
        }
    }
}
