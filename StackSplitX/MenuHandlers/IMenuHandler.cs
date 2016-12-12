﻿using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StackSplitX.MenuHandlers
{
    /// <summary>Represents states of input being handled.</summary>
    public enum EInputHandled
    {
        /// <summary>Input should be forwarded and the split menu should be closed.</summary>
        NotHandled,
        /// <summary>Input should be forwarded but the split menu should remain open.</summary>
        Handled,
        /// <summary>Input consumed by the handler, don't propogate it.</summary>
        Consumed
    }

    public interface IMenuHandler
    {
        /// <summary>Checks if the menu this handler wraps is open.</summary>
        /// <returns>True if it is open, false otherwise.</returns>
        bool IsOpen();

        /// <summary>Notifies the handler that it's native menu has been opened.</summary>
        /// <param name="menu">The menu that was opened.</param>
        void Open(IClickableMenu menu);

        /// <summary>Notifies the handler that it's native menu was closed.</summary>
        void Close();

        /// <summary>Runs on tick for handling things like highlighting text.</summary>
        void Update();

        /// <summary>Draws the split menu.</summary>
        void Draw(SpriteBatch spriteBatch);

        /// <summary>Tells the handler to close the split menu.</summary>
        void CloseSplitMenu();

        /// <summary>Handles mouse input.</summary>
        /// <param name="priorState">Prior state of the mouse.</param>
        /// <param name="newState">Current state of the mouse.</param>
        /// <returns>Enum value representing how the input was handled.</returns>
        EInputHandled HandleMouseInput(MouseState priorState, MouseState newState);

        /// <summary>Handles keyboard input.</summary>
        /// <param name="priorState">Prior state of the keyboard.</param>
        /// <param name="newState">Current state of the keyboard.</param>
        /// <returns>Enum value representing how the input was handled.</returns>
        EInputHandled HandleKeyboardInput(Keys keyPressed);
    }
}
