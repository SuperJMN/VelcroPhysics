using System.Text;
using FarseerGames.FarseerPhysics;
using FarseerGames.FarseerPhysics.Collisions;
using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysicsDemos.DrawingSystem;
using FarseerGames.FarseerPhysicsDemos.ScreenSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace FarseerGames.FarseerPhysicsDemos.Demos.Demo3
{
    public class Demo3Screen : GameScreen
    {
        private readonly LineBrush _lineBrush = new LineBrush(1, Color.Black); //used to draw spring on mouse grab

        private readonly PhysicsSimulator _physicsSimulator;
        private readonly PhysicsSimulatorView _physicsSimulatorView;
        private Body _agentBody;
        private Vector2 _agentCrossBeamOrigin;
        private Texture2D _agentCrossBeamTexture;
        private Geom[] _agentGeom;
        private Vector2 _agentOrigin;
        private Texture2D _agentTexture;
        private ContentManager _contentManager;
        private bool _debugViewEnabled;
        private bool _firstRun = true;

        private Body _floorBody;
        private Geom _floorGeom;
        private Vector2 _floorOrigin;
        private Texture2D _floorTexture;
        private FixedLinearSpring _mousePickSpring;

        private Body[] _obstacleBody;
        private Geom[] _obstacleGeom;
        private Vector2 _obstacleOrigin;
        private Texture2D _obstacleTexture;
        private Geom _pickedGeom;
        public float simTime;
        public bool updatedOnce;

        public Demo3Screen()
        {
            _physicsSimulator = new PhysicsSimulator(new Vector2(0, 100));
            _physicsSimulatorView = new PhysicsSimulatorView(_physicsSimulator);
            _physicsSimulatorView.EnableGridView = false;
        }

        public override void LoadContent()
        {
            if (_contentManager == null) _contentManager = new ContentManager(ScreenManager.Game.Services);
            _lineBrush.Load(ScreenManager.GraphicsDevice);
            _physicsSimulatorView.LoadContent(ScreenManager.GraphicsDevice, _contentManager);

            LoadAgent();
            LoadFloor();
            LoadObstacles();
        }

        public void LoadAgent()
        {
            _agentTexture = DrawingHelper.CreateCircleTexture(ScreenManager.GraphicsDevice, 16, Color.Gold, Color.Black);
            _agentOrigin = new Vector2(_agentTexture.Width/2f, _agentTexture.Height/2f);

            _agentCrossBeamTexture = DrawingHelper.CreateRectangleTexture(ScreenManager.GraphicsDevice, 16, 120,
                                                                         Color.DarkGray, Color.Black);
            _agentCrossBeamOrigin = new Vector2(_agentCrossBeamTexture.Width/2f, _agentCrossBeamTexture.Height/2f);

            _agentBody = BodyFactory.Instance.CreateRectangleBody(_physicsSimulator, 80, 80, 5);
            _agentBody.Position = new Vector2(ScreenManager.ScreenCenter.X, 110);

            _agentGeom = new Geom[7];
            _agentGeom[0] = GeomFactory.Instance.CreateCircleGeom(_physicsSimulator, _agentBody, 16, 10,
                                                                 new Vector2(-40, -40), 0);
            _agentGeom[0].RestitutionCoefficient = .2f;
            _agentGeom[0].FrictionCoefficient = .2f;
            _agentGeom[0].CollisionGroup = 1;
            _agentGeom[1] = GeomFactory.Instance.CreateGeom(_physicsSimulator, _agentBody, _agentGeom[0],
                                                           new Vector2(-40, 40), 0);
            _agentGeom[2] = GeomFactory.Instance.CreateGeom(_physicsSimulator, _agentBody, _agentGeom[0],
                                                           new Vector2(40, -40), 0);
            _agentGeom[3] = GeomFactory.Instance.CreateGeom(_physicsSimulator, _agentBody, _agentGeom[0],
                                                           new Vector2(40, 40), 0);
            _agentGeom[4] = GeomFactory.Instance.CreateGeom(_physicsSimulator, _agentBody, _agentGeom[0], new Vector2(0, 0),
                                                           0);

            _agentGeom[5] = GeomFactory.Instance.CreateRectangleGeom(_physicsSimulator, _agentBody, 16, 130, Vector2.Zero,
                                                                    MathHelper.PiOver4);
            _agentGeom[5].CollisionGroup = 1;
            _agentGeom[6] = GeomFactory.Instance.CreateRectangleGeom(_physicsSimulator, _agentBody, 16, 130, Vector2.Zero,
                                                                    -MathHelper.PiOver4);
            _agentGeom[6].CollisionGroup = 1;
        }

        public void LoadFloor()
        {
            //load texture that will visually represent the physics body
            _floorTexture = DrawingHelper.CreateRectangleTexture(ScreenManager.GraphicsDevice, ScreenManager.ScreenWidth,
                                                                100, Color.White, Color.Black);
            _floorOrigin = new Vector2(_floorTexture.Width/2f, _floorTexture.Height/2f);

            //use the body factory to create the physics body
            _floorBody = BodyFactory.Instance.CreateRectangleBody(_physicsSimulator, ScreenManager.ScreenWidth, 100, 1);
            _floorBody.IsStatic = true;
            _floorGeom = GeomFactory.Instance.CreateRectangleGeom(_physicsSimulator, _floorBody, ScreenManager.ScreenWidth,
                                                                 100);
            _floorGeom.RestitutionCoefficient = .2f;
            _floorGeom.FrictionCoefficient = .2f;
            _floorBody.Position = new Vector2(ScreenManager.ScreenCenter.X, ScreenManager.ScreenHeight - 50);
        }

        public void LoadObstacles()
        {
            _obstacleTexture = DrawingHelper.CreateRectangleTexture(ScreenManager.GraphicsDevice, 128, 32, Color.White,
                                                                   Color.Black);
            _obstacleOrigin = new Vector2(_obstacleTexture.Width/2f, _obstacleTexture.Height/2f);

            _obstacleBody = new Body[5];
            _obstacleGeom = new Geom[5];
            for (int i = 0; i < _obstacleBody.Length; i++)
            {
                _obstacleBody[i] = BodyFactory.Instance.CreateRectangleBody(_physicsSimulator, 128, 32, 1);
                _obstacleBody[i].IsStatic = true;

                if (i == 0)
                {
                    _obstacleGeom[i] = GeomFactory.Instance.CreateRectangleGeom(_physicsSimulator, _obstacleBody[i], 128,
                                                                               32);
                    _obstacleGeom[i].RestitutionCoefficient = .2f;
                    _obstacleGeom[i].FrictionCoefficient = .2f;
                }
                else
                {
                    _obstacleGeom[i] = GeomFactory.Instance.CreateGeom(_physicsSimulator, _obstacleBody[i], _obstacleGeom[0]);
                }
            }

            _obstacleBody[0].Position = ScreenManager.ScreenCenter + new Vector2(-50, -200);
            _obstacleBody[1].Position = ScreenManager.ScreenCenter + new Vector2(150, -100);
            _obstacleBody[2].Position = ScreenManager.ScreenCenter + new Vector2(100, 50);
            _obstacleBody[3].Position = ScreenManager.ScreenCenter + new Vector2(-100, 200);
            _obstacleBody[4].Position = ScreenManager.ScreenCenter + new Vector2(-170, 0);
        }

        public override void UnloadContent()
        {
            _contentManager.Unload();
            _physicsSimulator.Clear();
        }

        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            if (IsActive)
            {
                _physicsSimulator.Update(gameTime.ElapsedGameTime.Milliseconds*.001f);
                updatedOnce = true;
            }
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        }

        public override void Draw(GameTime gameTime)
        {
            if (!updatedOnce)
            {
                return;
            }
            ScreenManager.SpriteBatch.Begin(SpriteBlendMode.AlphaBlend);
            ScreenManager.SpriteBatch.Draw(_floorTexture, _floorBody.Position, null, Color.White, _floorBody.Rotation,
                                           _floorOrigin, 1, SpriteEffects.None, 0f);
            DrawObstacles();
            DrawAgent();

            if (_mousePickSpring != null)
            {
                _lineBrush.Draw(ScreenManager.SpriteBatch,
                               _mousePickSpring.Body.GetWorldPosition(_mousePickSpring.BodyAttachPoint),
                               _mousePickSpring.WorldAttachPoint);
            }
            if (_debugViewEnabled)
            {
                _physicsSimulatorView.Draw(ScreenManager.SpriteBatch);
            }
            ScreenManager.SpriteBatch.End();
        }

        private void DrawAgent()
        {
            for (int i = 5; i < 7; i++)
            {
                ScreenManager.SpriteBatch.Draw(_agentCrossBeamTexture, _agentGeom[i].Position, null, Color.White,
                                               _agentGeom[i].Rotation, _agentCrossBeamOrigin, 1, SpriteEffects.None, 0f);
            }
            for (int i = 0; i < 5; i++)
            {
                ScreenManager.SpriteBatch.Draw(_agentTexture, _agentGeom[i].Position, null, Color.White,
                                               _agentGeom[i].Rotation, _agentOrigin, 1, SpriteEffects.None, 0f);
            }
        }

        private void DrawObstacles()
        {
            for (int i = 0; i < _obstacleBody.Length; i++)
            {
                ScreenManager.SpriteBatch.Draw(_obstacleTexture, _obstacleBody[i].Position, null, Color.White,
                                               _obstacleBody[i].Rotation, _obstacleOrigin, 1, SpriteEffects.None, 0f);
            }
        }

        public override void HandleInput(InputState input)
        {
            if (_firstRun)
            {
                ScreenManager.AddScreen(new PauseScreen(GetTitle(), GetDetails()));
                _firstRun = false;
            }
            if (input.PauseGame)
            {
                ScreenManager.AddScreen(new PauseScreen(GetTitle(), GetDetails()));
            }
            if (input.CurrentGamePadState.IsConnected)
            {
                HandleGamePadInput(input);
            }
            else
            {
                HandleKeyboardInput(input);
#if !XBOX
                HandleMouseInput(input);
#endif
            }
            base.HandleInput(input);
        }

        private void HandleGamePadInput(InputState input)
        {
            if (input.LastGamePadState.Buttons.Y != ButtonState.Pressed &&
                input.CurrentGamePadState.Buttons.Y == ButtonState.Pressed)
            {
                _debugViewEnabled = !_debugViewEnabled;
                _physicsSimulator.EnableDiagnostics = _debugViewEnabled;
            }

            Vector2 force = 800*input.CurrentGamePadState.ThumbSticks.Left;
            force.Y = -force.Y;
            _agentBody.ApplyForce(force);

            float rotation = -8000*input.CurrentGamePadState.Triggers.Left;
            _agentBody.ApplyTorque(rotation);

            rotation = 8000*input.CurrentGamePadState.Triggers.Right;
            _agentBody.ApplyTorque(rotation);
        }

        private void HandleKeyboardInput(InputState input)
        {
            if (!input.LastKeyboardState.IsKeyDown(Keys.F1) && input.CurrentKeyboardState.IsKeyDown(Keys.F1))
            {
                _debugViewEnabled = !_debugViewEnabled;
                _physicsSimulator.EnableDiagnostics = _debugViewEnabled;
            }

            const float forceAmount = 800;
            Vector2 force = Vector2.Zero;
            force.Y = -force.Y;
            if (input.CurrentKeyboardState.IsKeyDown(Keys.A))
            {
                force += new Vector2(-forceAmount, 0);
            }
            if (input.CurrentKeyboardState.IsKeyDown(Keys.S))
            {
                force += new Vector2(0, forceAmount);
            }
            if (input.CurrentKeyboardState.IsKeyDown(Keys.D))
            {
                force += new Vector2(forceAmount, 0);
            }
            if (input.CurrentKeyboardState.IsKeyDown(Keys.W))
            {
                force += new Vector2(0, -forceAmount);
            }

            _agentBody.ApplyForce(force);

            const float torqueAmount = 8000;
            float torque = 0;
            if (input.CurrentKeyboardState.IsKeyDown(Keys.Left))
            {
                torque -= torqueAmount;
            }
            if (input.CurrentKeyboardState.IsKeyDown(Keys.Right))
            {
                torque += torqueAmount;
            }
            _agentBody.ApplyTorque(torque);
        }


#if !XBOX
        private void HandleMouseInput(InputState input)
        {
            Vector2 point = new Vector2(input.CurrentMouseState.X, input.CurrentMouseState.Y);
            if (input.LastMouseState.LeftButton == ButtonState.Released &&
                input.CurrentMouseState.LeftButton == ButtonState.Pressed)
            {
                //create mouse spring
                _pickedGeom = _physicsSimulator.Collide(point);
                if (_pickedGeom != null)
                {
                    _mousePickSpring = ControllerFactory.Instance.CreateFixedLinearSpring(_physicsSimulator,
                                                                                         _pickedGeom.Body,
                                                                                         _pickedGeom.Body.
                                                                                             GetLocalPosition(point),
                                                                                         point, 20, 10);
                }
            }
            else if (input.LastMouseState.LeftButton == ButtonState.Pressed &&
                     input.CurrentMouseState.LeftButton == ButtonState.Released)
            {
                //destroy mouse spring
                if (_mousePickSpring != null && _mousePickSpring.IsDisposed == false)
                {
                    _mousePickSpring.Dispose();
                    _mousePickSpring = null;
                }
            }

            //move anchor point
            if (input.CurrentMouseState.LeftButton == ButtonState.Pressed && _mousePickSpring != null)
            {
                _mousePickSpring.WorldAttachPoint = point;
            }
        }
#endif

        public string GetTitle()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Static Bodies And ");
            sb.AppendLine("Offset Geometries");
            return sb.ToString();
        }

        public string GetDetails()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("This demo shows a single body with multiple geometry");
            sb.AppendLine("objects attached.  The yellow circles are offset");
            sb.AppendLine("from the bodies center. The body itself is created");
            sb.AppendLine("using 'CreateRectangleBody' so that it's moment of");
            sb.AppendLine("inertia is that of a rectangle.");
            sb.AppendLine("");
            sb.AppendLine("This demo also shows the use of static bodies.");
            sb.AppendLine("");
            sb.AppendLine("GamePad:");
            sb.AppendLine("  -Rotate : left and right triggers");
            sb.AppendLine("  -Move : left thumbstick");
            sb.AppendLine("");
            sb.AppendLine("Keyboard:");
            sb.AppendLine("  -Rotate : left and right arrows");
            sb.AppendLine("  -Move : A,S,D,W");
            sb.AppendLine("");
            sb.AppendLine("Mouse");
            sb.AppendLine("  -Hold down left button and drag");
            return sb.ToString();
        }
    }
}