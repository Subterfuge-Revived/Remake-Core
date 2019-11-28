using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using SubterfugeFrontend.Shared.Content.Game.Events.Listeners.InputEvents.MouseEvents;
using System;
using System.Collections.Generic;
using System.Text;

namespace SubterfugeFrontend.Shared.Content.Game.Events.Listeners
{
    class MouseListener : IListener
    {
        private MouseState _currentState;
        private bool _dragging;
        private GameTime _gameTime;
        private bool _hasDoubleClicked;
        private MouseButtonEvent _mouseDownArgs;
        private MouseButtonEvent _previousClickArgs;
        private MouseState _previousState;


        private int DragThreshold = 2;

        public event EventHandler<MouseButtonEvent> MouseRightDown;
        public event EventHandler<MouseButtonEvent> MouseRightUp;
        public event EventHandler<MouseButtonEvent> MouseClicked;
        // public event EventHandler<MouseButtonEvent> MouseDoubleClicked;

        public event EventHandler<MouseMoveEvent> MouseMoved;
        public event EventHandler<MouseButtonEvent> MouseWheelMoved;

        public event EventHandler<MouseDragEvent> MouseDragStart;
        public event EventHandler<MouseDragEvent> MouseDrag;
        public event EventHandler<MouseButtonEvent> MouseDragEnd;

        public void listen()
        {
            _currentState = Mouse.GetState();

            // Translate the mouse location to world coordinates

            CheckButtonPressed(s => s.LeftButton, MouseButton.Left);
            CheckButtonPressed(s => s.MiddleButton, MouseButton.Middle);
            CheckButtonPressed(s => s.RightButton, MouseButton.Right);
            CheckButtonPressed(s => s.XButton1, MouseButton.XButton1);
            CheckButtonPressed(s => s.XButton2, MouseButton.XButton2);

            CheckButtonReleased(s => s.LeftButton, MouseButton.Left);
            CheckButtonReleased(s => s.MiddleButton, MouseButton.Middle);
            CheckButtonReleased(s => s.RightButton, MouseButton.Right);
            CheckButtonReleased(s => s.XButton1, MouseButton.XButton1);
            CheckButtonReleased(s => s.XButton2, MouseButton.XButton2);

            // Check for any sort of mouse movement.
            if (HasMouseMoved)
            {
                MouseMoved?.Invoke(this,
                    new MouseMoveEvent(_previousState, _currentState));

                CheckMouseDragged(s => s.LeftButton, MouseButton.Left);
                CheckMouseDragged(s => s.MiddleButton, MouseButton.Middle);
                CheckMouseDragged(s => s.RightButton, MouseButton.Right);
                CheckMouseDragged(s => s.XButton1, MouseButton.XButton1);
                CheckMouseDragged(s => s.XButton2, MouseButton.XButton2);
            }
        }

        /// <summary>
        ///     Returns true if the mouse has moved between the current and previous frames.
        /// </summary>
        /// <value><c>true</c> if the mouse has moved; otherwise, <c>false</c>.</value>
        public bool HasMouseMoved => (_previousState.X != _currentState.X) || (_previousState.Y != _currentState.Y);
        private void CheckButtonPressed(Func<MouseState, ButtonState> getButtonState, MouseButton button)
        {
            if ((getButtonState(_currentState) == ButtonState.Pressed) &&
                (getButtonState(_previousState) == ButtonState.Released))
            {
                var args = new MouseButtonEvent(_previousState, _currentState, button);

                MouseRightDown?.Invoke(this, args);
                _mouseDownArgs = args;
            }
        }

        private void CheckButtonReleased(Func<MouseState, ButtonState> getButtonState, MouseButton button)
        {
            if ((getButtonState(_currentState) == ButtonState.Released) &&
                (getButtonState(_previousState) == ButtonState.Pressed))
            {
                var args = new MouseButtonEvent(_previousState, _currentState, button);

                if (_mouseDownArgs.pressedButton == args.pressedButton)
                {
                    var clickMovement = DistanceBetween(args.getMouseLocation(), _mouseDownArgs.getMouseLocation());

                    // If the mouse hasn't moved much between mouse down and mouse up
                    if (clickMovement < DragThreshold)
                    {
                        if (!_hasDoubleClicked)
                            MouseClicked?.Invoke(this, args);
                    }
                    else // If the mouse has moved between mouse down and mouse up
                    {
                        MouseDragEnd?.Invoke(this, args);
                        _dragging = false;
                    }
                }

                MouseRightUp?.Invoke(this, args);

                _hasDoubleClicked = false;
                _previousClickArgs = args;
            }
        }

        private void CheckMouseDragged(Func<MouseState, ButtonState> getButtonState, MouseButton button)
        {
            if ((getButtonState(_currentState) == ButtonState.Pressed) &&
                (getButtonState(_previousState) == ButtonState.Pressed))
            {
                var args = new MouseDragEvent(_previousState, _currentState, button);

                if (_mouseDownArgs.pressedButton == args.pressedButton)
                {
                    if (_dragging)
                        MouseDrag?.Invoke(this, args);
                    else
                    {
                        // Only start to drag based on DragThreshold
                        var clickMovement = args.getDelta();

                        if (clickMovement.ToVector2().Length() > DragThreshold)
                        {
                            _dragging = true;
                            MouseDragStart?.Invoke(this, args);
                        }
                    }
                }
            }
        }

        private static int DistanceBetween(Point a, Point b)
        {
            return Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);
        }

    }
}
