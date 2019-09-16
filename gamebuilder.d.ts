// see docs/typechecking.md

import * as THREE from "three";

declare global {

    interface propsAreInbuild { [key: string]: any }
    const props: propsAreInbuild;

    type ActorRef = { comment: "actually a string", docHack: "else vscode hints 'string'" };

    // functions

    function clone(original, pos?, rot?): ActorRef;
    function deltaTime(): number;
    function detachFromParentPlease(actor: ActorRef): void;
    function detachFromParentPlease(actor: ActorRef): void;
    function getActorGroupDescription(pattern: string): string;
    function getActors(): ActorRef[];
    function getActorsInGroup(pattern: string): ActorRef[];
    function getActorsWithTag(tag: string): ActorRef[];
    function getBoundsRadiusOuter(actor: ActorRef, includeVertical?: boolean): number;
    function getBoundsRadiusOuter(includeVertical?: boolean): number;
    function getCardTargetActor(propName: string, actionMessage: any): ActorRef | null;
    function getDisplayName(actor: ActorRef): string;
    function getParent(actor?: ActorRef): ActorRef | null;
    function getPointBehind(dist: number, actor?: ActorRef): THREE.Vector3;
    function getPos(actor?: ActorRef): THREE.Vector3;
    function getThrottle(actor: ActorRef): THREE.Vector3;
    function getTimeSinceReset();
    function getVar(name: string, actor: ActorRef): any;
    function getVelocity(actor: ActorRef): THREE.Vector3;
    function isClone(actor?): boolean;
    function log(msg: string): void;
    function lookAt(targetActorOrPoint: ActorRef | THREE.Vector3, yawOnly?: boolean, maxRadians?: number): void;
    function myself(): ActorRef;
    function overlapSphere(center: THREE.Vector3, radius: number): ActorRef;
    function propActor(prop: string): ActorRef;
    function propActorGroup(prop: string);
    function propEnum(name: string, defaultValue: string, allowedValues: string[], options?);
    function propNumber(prop: string, val: number);
    function send(actor: ActorRef, messageName: string, arg?: any): void;
    function setKinematic(isKinematic: boolean);
    function setPos(worldPos: THREE.Vector3): void;
    function setTintHex(colorHex: string): void;
    function setVar(name: string, value: any): void;
    function turn(radians: number, axis?: THREE.Vector3): void;
    function vec3(x: number, y: number, z: number): THREE.Vector3;
}
