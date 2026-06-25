
# Implementing a Haptic Emitter / Receiver in a scene
The local player avatar should have a game object for each body part that should receive haptics with the following components:
- Collider
- PHap_HapticReceiver (body part selected)
  - A child game object assigned as Body Part origin (0,0,0), with Z of it's transform being it's "front".

The remote avatar bodyparts, or any object touching the player that should provide haptics, should have the following objects:
- Collider with IsTrigger turned on
- Rigidbody (with isKinematic turned on if it is not a physics object)
- PHap_HapticEmitter
- PHap_HapticEffect (with Base effect assigned)


