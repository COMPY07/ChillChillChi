public class NodePosition {
        
    public float x, y;
        
    public NodePosition(float x, float y) {
        this.x = x;
        this.y = y;
    }
        
    public override string ToString() {
        return $"{this.x}, {this.y}";
    }
}