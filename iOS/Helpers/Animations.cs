using CoreGraphics;
using UIKit;

namespace PK.iOS.Helpers
{
   public class CustomAnimatedTransitioning : UIViewControllerAnimatedTransitioning
   {
      private readonly bool isPresenting;
      private readonly bool isReverseAnimation;

      public CustomAnimatedTransitioning( bool isPresenting, bool isReverseAnimation )
      {
         this.isPresenting = isPresenting;
         this.isReverseAnimation = isReverseAnimation;
      }

      public override double TransitionDuration( IUIViewControllerContextTransitioning transitionContext )
      {
         return UINavigationController.HideShowBarDuration;
      }

      public override void AnimateTransition( IUIViewControllerContextTransitioning transitionContext )
      {
         //var fromViewController = transitionContext.GetViewControllerForKey( UITransitionContext.FromViewControllerKey );
         //var toViewController = transitionContext.GetViewControllerForKey( UITransitionContext.ToViewControllerKey );

         var fromView = transitionContext.GetViewFor( UITransitionContext.FromViewKey );
         var toView = transitionContext.GetViewFor( UITransitionContext.ToViewKey );

         double transitionDuration = TransitionDuration( transitionContext );

         var containerView = transitionContext.ContainerView;
         var originalToViewFrame = toView.Frame;

         toView.Alpha = 0;

         // Set intial positioning for views
         if( isPresenting )
         {
            containerView.AddSubview( toView );
            toView.Frame = new CGRect( x: isReverseAnimation ? -toView.Frame.Width : toView.Frame.Width, 
               y: toView.Frame.Y, width: toView.Frame.Width, height: toView.Frame.Height );
         }
         else
         {
            containerView.InsertSubviewBelow( toView, siblingSubview: fromView );
            toView.Frame = new CGRect( x: isReverseAnimation ? toView.Frame.Width : -toView.Frame.Width, 
               y: toView.Frame.Y, width: toView.Frame.Width, height: toView.Frame.Height );
         }

         UIView.AnimateNotify( duration: transitionDuration, delay: 0, options: UIViewAnimationOptions.LayoutSubviews,
            animation: ( ) => {

               toView.Alpha = 1;
               fromView.Alpha = 0;
               toView.Frame = originalToViewFrame;

            },
            completion: ( bool finished ) => {

               transitionContext.CompleteTransition( !transitionContext.TransitionWasCancelled );

            } );
      }
   }
}
