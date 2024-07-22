# Deap genetic

import random
from deap import base, creator, tools
import bpy
import bmesh
import mathutils
import math

GENETIC_ITERATOR = None


def createNoiseNode(material, scale, detail, roughness, distortion):
    noise = material.node_tree.nodes.new('ShaderNodeTexNoise')
    noise.inputs['Scale'].default_value = scale
    noise.inputs['Detail'].default_value = detail
    noise.inputs['Roughness'].default_value = roughness
    noise.inputs['Distortion'].default_value = distortion
    return noise


def createCheckerNode(material, scale):
    checker = material.node_tree.nodes.new('ShaderNodeTexChecker')
    checker.inputs['Scale'].default_value = scale
    return checker


def createMagicNode(material, depth, scale, distortion):
    magic = material.node_tree.nodes.new('ShaderNodeTexMagic')
    # magic.inputs['Depth'].default_value = depth
    magic.inputs['Scale'].default_value = scale
    magic.inputs['Distortion'].default_value = distortion
    return magic


def createGradientNode(material, gradient_type):
    gradient = material.node_tree.nodes.new('ShaderNodeTexGradient')
    if gradient_type == 0:
        gradient.gradient_type = 'LINEAR'
    elif gradient_type == 1:
        gradient.gradient_type = 'QUADRATIC'
    elif gradient_type == 2:
        gradient.gradient_type = 'EASING'
    elif gradient_type == 3:
        gradient.gradient_type = 'DIAGONAL'
    elif gradient_type == 4:
        gradient.gradient_type = 'SPHERICAL'
    elif gradient_type == 5:
        gradient.gradient_type = 'QUADRATIC_SPHERE'
    elif gradient_type == 6:
        gradient.gradient_type = 'RADIAL'
    return gradient


def blenderCreateTex(individual, n):
    # Get individual's genes
    bsdf_metallic = individual[0]
    bsdf_specular = individual[1]
    bsdf_roughness = individual[2]

    color_ramp_1_1 = (individual[3], individual[4], individual[5], 1.0)
    color_ramp_1_2 = (individual[6], individual[7], individual[8], 1.0)
    color_ramp_1_1_pos = individual[9]
    color_ramp_1_2_pos = individual[10]

    color_ramp_2_1 = (individual[11], individual[12], individual[13], 1.0)
    color_ramp_2_2 = (individual[14], individual[15], individual[16], 1.0)
    color_ramp_2_1_pos = individual[17]
    color_ramp_2_2_pos = individual[18]

    # Texture Mixing:
    # 0 - Noise Texture
    # 1 - Checker Texture
    # 2 - Magic Texture
    # 3 - Gradient Texture
    texture_mixing_1 = individual[19]
    texture_mixing_2 = individual[20]
    texture_mixing_fac = individual[21]

    # Noise Texture
    base_color_noise_scale = individual[22]
    base_color_noise_detail = individual[23]
    base_color_noise_roughness = individual[24]
    base_color_noise_distortion = individual[25]

    # Checker Texture
    checker_scale = individual[26]

    # Magic Texture
    magic_depth = individual[27]
    magic_scale = individual[28]
    magic_distortion = individual[29]

    # Gradient Texture
    # 0 - Linear
    # 1 - Quadratic
    # 2 - Easing
    # 3 - Diagonal
    # 4 - Spherical
    # 5 - Quadratic Sphere
    # 6 - Radial
    gradient_type = individual[30]

    # Create material
    mat = bpy.data.materials.new(name="Genetic Material " + str(n))


    # Set material properties
    mat.use_nodes = True
    bsdf = mat.node_tree.nodes.get('Principled BSDF')
    bsdf.inputs['Metallic'].default_value = bsdf_metallic

    # Switch depending on blender version:
    #3.x:
    if 'Specular' in bsdf.inputs:
        bsdf.inputs['Specular'].default_value = bsdf_specular
    # 4.x:
    else:
        # Specular IOR level
        bsdf.inputs[12].default_value = bsdf_specular

    bsdf.inputs['Roughness'].default_value = bsdf_roughness

    # Color ramp 1
    color_ramp_1 = mat.node_tree.nodes.new('ShaderNodeValToRGB')
    color_ramp_1.color_ramp.elements[0].color = color_ramp_1_1
    color_ramp_1.color_ramp.elements[0].position = color_ramp_1_1_pos
    color_ramp_1.color_ramp.elements[1].color = color_ramp_1_2
    color_ramp_1.color_ramp.elements[1].position = color_ramp_1_2_pos

    # Color ramp 2
    color_ramp_2 = mat.node_tree.nodes.new('ShaderNodeValToRGB')
    color_ramp_2.color_ramp.elements[0].color = color_ramp_2_1
    color_ramp_2.color_ramp.elements[0].position = color_ramp_2_1_pos
    color_ramp_2.color_ramp.elements[1].color = color_ramp_2_2
    color_ramp_2.color_ramp.elements[1].position = color_ramp_2_2_pos

    # Texture Mixing
    mixing_node = mat.node_tree.nodes.new('ShaderNodeMixRGB')
    mixing_node.blend_type = 'MIX'
    # Connect color ramps to color inputs
    mat.node_tree.links.new(color_ramp_1.outputs[0], mixing_node.inputs[1])
    mat.node_tree.links.new(color_ramp_2.outputs[0], mixing_node.inputs[2])
    # Connect mix to bsdf base color
    mat.node_tree.links.new(mixing_node.outputs[0], bsdf.inputs['Base Color'])

    # switch depending on texture mixing
    nodes = []
    types = [texture_mixing_1, texture_mixing_2, texture_mixing_fac]
    for i in range(3):
        if types[i] == 0:
            noise = createNoiseNode(mat, base_color_noise_scale, base_color_noise_detail, base_color_noise_roughness,
                                    base_color_noise_distortion)
            nodes.append(noise)
        elif types[i] == 1:
            checker = createCheckerNode(mat, checker_scale)
            nodes.append(checker)
        elif types[i] == 2:
            magic = createMagicNode(mat, magic_depth, magic_scale, magic_distortion)
            nodes.append(magic)
        elif types[i] == 3:
            gradient = createGradientNode(mat, gradient_type)
            nodes.append(gradient)

    # Connect mixing nodes 1&2 to color ramps 1 fac to mixing fac

    mat.node_tree.links.new(nodes[0].outputs[0], color_ramp_1.inputs[0])
    mat.node_tree.links.new(nodes[1].outputs[0], color_ramp_2.inputs[0])
    mat.node_tree.links.new(nodes[2].outputs[0], mixing_node.inputs[0])

    return mat

def createBone(armature, parent_bone, head, tail):
    # Exit all modes
    bpy.ops.object.mode_set(mode='OBJECT')
    bpy.ops.object.select_all(action='DESELECT')


    # Select the armature
    bpy.context.view_layer.objects.active = armature
    bpy.ops.object.mode_set(mode='EDIT')
    bpy.ops.armature.select_all(action='DESELECT')

    # Create a new bone
    bone = armature.data.edit_bones.new("Bone")
    bone.head = head
    bone.tail = tail

    if parent_bone is not None:
        bone.parent = parent_bone

    # Exit edit mode
    bpy.ops.object.mode_set(mode='OBJECT')
    return bone


def closestBone(armature, facePos):
    """
    Cherche le bone le plus proche d'une face
        - armature :  bpy.types.Armature
        - facePos : Vector3D ( trouvable avec face.calc_center_bounds() avec face : bmesh.types.BMFace)

    Merci éric et HD les sangs
    """
    # res = tuple( (distance entre face et bone), bone )
    res = min([(math.dist(facePos, bone.tail), bone) for bone in armature.data.edit_bones], key=lambda x: x[0])

    return res[1]

def blenderDraw(form_individual, mat_individual, n_extrusions, n, location=(0, 0, 0)):
    # Start by creating a new cube mesh at location with name "Individual n"
    bpy.ops.mesh.primitive_cube_add(location=location)
    obj = bpy.context.active_object
    obj.name = "Individual " + str(n)
    armature = bpy.data.armatures.new("Armature")
    obj_armature = bpy.data.objects.new("Armature", armature)
    bpy.context.collection.objects.link(obj_armature)
    obj_armature.location = obj.location
    obj_armature.name = "Armature " + str(n)

    endling_bones = set()
    createBone(obj_armature, None, (0, 0, 0), (0, 0, 0.1))

    bpy.context.view_layer.objects.active = obj_armature
    bpy.ops.object.mode_set(mode='EDIT')
    bpy.ops.armature.select_all(action='SELECT')

    obj_armature.data.edit_bones[-1].name = "Root"

    bpy.ops.armature.select_all(action='DESELECT')
    bpy.ops.object.mode_set(mode='OBJECT')

    for ext in range(0, n_extrusions):
        # From object context, deselect all objects
        bpy.ops.object.select_all(action='DESELECT')
        # Select obj and enter edit mode
        bpy.context.view_layer.objects.active = obj
        bpy.ops.object.mode_set(mode='EDIT')

        # get current number of faces
        bm = bmesh.from_edit_mesh(obj.data)
        n_faces = len(bm.faces)

        # Get individual's genes
        face_id = form_individual[ext * 3] % n_faces
        scale = form_individual[ext * 3 + 1]
        extrude = form_individual[ext * 3 + 2]

        # Select face
        bm.faces.ensure_lookup_table()
        bmesh.update_edit_mesh(obj.data)

        face_center = bm.faces[face_id].calc_center_bounds()
        face_normal = bm.faces[face_id].normal.normalized()

        bpy.ops.mesh.select_all(action='DESELECT')
        bm.faces[face_id].select = True

        extrude_vector = extrude * face_normal

        # Extrude face
        bpy.ops.mesh.extrude_region_move(TRANSFORM_OT_translate={
            "value": extrude_vector
        })
        bm.faces.ensure_lookup_table()

        # create bone
        # exit all modes
        bpy.ops.object.mode_set(mode='OBJECT')
        bpy.ops.object.select_all(action='DESELECT')

        # Select the armature
        bpy.context.view_layer.objects.active = obj_armature
        bpy.ops.object.mode_set(mode='EDIT')
        bpy.ops.armature.select_all(action='SELECT')

        closest = closestBone(obj_armature, face_center)

        bpy.ops.armature.select_all(action="DESELECT")
        obj_armature.data.edit_bones.active = closest
        closest.select_tail = True

        # Compute distance between tail and face center and add half of extrude
        dist = math.dist(closest.tail, face_center) + extrude / 2

        # Create a new bone
        bpy.ops.armature.extrude_move(TRANSFORM_OT_translate={"value": (dist * face_normal)})

        # get newest bone to add contraints to it later
        obj_armature.data.edit_bones[-1].name = "Bone " + str(ext)
        endling_bones.add(obj_armature.data.edit_bones[-1].name)

        # remove the parent bone from the endling bones
        endling_bones.discard(closest.name)

        # Exit edit mode
        bpy.ops.object.mode_set(mode='OBJECT')



    # Select the object
    bpy.ops.object.mode_set(mode='OBJECT')
    bpy.context.view_layer.objects.active = obj
    bpy.ops.object.select_all(action='DESELECT')
    obj.select_set(True)

    # Add subdivision modifier
    bpy.ops.object.modifier_add(type='SUBSURF')
    obj.modifiers["Subdivision"].levels = 2
    obj.modifiers["Subdivision"].render_levels = 2

    # Apply modifier
    bpy.ops.object.modifier_apply(modifier="Subdivision")

    # Add smooth shading
    bpy.ops.object.shade_smooth()

    # Apply automatic weights
    bpy.ops.object.mode_set(mode='OBJECT')
    bpy.ops.object.select_all(action='DESELECT')

    # select object
    bpy.context.view_layer.objects.active = obj
    obj.select_set(True)

    # select armature
    bpy.context.view_layer.objects.active = obj_armature
    obj_armature.select_set(True)

    # parent with automatic weights
    bpy.ops.object.parent_set(type='ARMATURE_AUTO')

    # deselect all
    bpy.ops.object.select_all(action='DESELECT')

    # for each endling bone add ik constraint

    bpy.context.view_layer.objects.active = obj_armature
    bpy.ops.object.mode_set(mode='POSE')
    for bone in endling_bones:
        # Help
        bpy.context.active_object.pose.bones[bone].bone.select = True
        bpy.types.ArmatureBones.active = bpy.context.active_object.pose.bones[bone]
        bpy.context.active_object.data.bones.active = bpy.context.active_object.pose.bones[bone].bone
        bpy.ops.pose.constraint_add(type='IK')

        chain_num = 1
        cur_bon = bpy.context.active_object.pose.bones[bone].parent
        while len(cur_bon.children) <= 1:
            chain_num += 1
            cur_bon = cur_bon.parent

        bpy.context.active_object.pose.bones[bone].constraints["IK"].chain_count = chain_num
        bpy.context.active_object.pose.bones[bone].bone.select = False




    # Exit pose mode
    bpy.ops.pose.select_all(action='DESELECT')
    bpy.ops.object.mode_set(mode='OBJECT')

    mat = blenderCreateTex(mat_individual, n)
    
    # Set as active material 
    obj.data.materials.append(mat)
    obj.active_material = mat

    return (obj, mat, armature)


def genetic(pop_size=20, n_extrusions=20, face_max=1000, scale_range=(0.5, 1.5, 0.0), extrude_range=(0.5, 1.5, 0.0)):
    toolbox = base.Toolbox()

    # Creator create classes with our fitness function and weight.
    creator.create("FitnessMax", base.Fitness, weights=(1.0,))
    creator.create("Individual", list, fitness=creator.FitnessMax)

    # We define that our individuals will be composed of integers and floats:
    # Genes are:
    # n_extrusions integers within 0 and face_max
    # n_extrusions floats within scale_range
    # n_extrusion floats  within extrude_range.
    toolbox.register("faces", random.randint, 0, face_max)
    toolbox.register("scale", random.uniform, scale_range[0], scale_range[1])
    toolbox.register("extrude", random.uniform, extrude_range[0], extrude_range[1])

    # bsdf properties
    toolbox.register("bsdf_metallic", random.uniform, 0, 1)
    toolbox.register("bsdf_specular", random.uniform, 0, 1)
    toolbox.register("bsdf_roughness", random.uniform, 0, 1)

    ### Color ramp 1

    toolbox.register("color_ramp_1_1_r", random.uniform, 0, 1)
    toolbox.register("color_ramp_1_1_b", random.uniform, 0, 1)
    toolbox.register("color_ramp_1_1_g", random.uniform, 0, 1)
    toolbox.register("color_ramp_1_2_r", random.uniform, 0, 1)
    toolbox.register("color_ramp_1_2_g", random.uniform, 0, 1)
    toolbox.register("color_ramp_1_2_b", random.uniform, 0, 1)
    toolbox.register("color_ramp_1_1_pos", random.uniform, 0, 1)
    toolbox.register("color_ramp_1_2_pos", random.uniform, 0, 1)

    ### Color ramp 2

    toolbox.register("color_ramp_2_1_r", random.uniform, 0, 1)
    toolbox.register("color_ramp_2_1_b", random.uniform, 0, 1)
    toolbox.register("color_ramp_2_1_g", random.uniform, 0, 1)
    toolbox.register("color_ramp_2_2_r", random.uniform, 0, 1)
    toolbox.register("color_ramp_2_2_g", random.uniform, 0, 1)
    toolbox.register("color_ramp_2_2_b", random.uniform, 0, 1)
    toolbox.register("color_ramp_2_1_pos", random.uniform, 0, 1)
    toolbox.register("color_ramp_2_2_pos", random.uniform, 0, 1)

    # Texture Mixing:
    # 0 - Noise Texture
    # 1 - Checker Texture
    # 2 - Magic Texture
    # 3 - Gradient Texture

    toolbox.register("texture_mixing_1", random.randint, 0, 3)
    toolbox.register("texture_mixing_2", random.randint, 0, 3)
    toolbox.register("texture_mixing_fac", random.randint, 0, 3)

    ### 0 - Genes used when using noise texture

    toolbox.register("base_color_noise_scale", random.uniform, 0, 50)
    toolbox.register("base_color_noise_detail", random.uniform, 0, 15)
    toolbox.register("base_color_noise_roughness", random.uniform, 0, 20)
    toolbox.register("base_color_noise_distortion", random.uniform, -10, 10)

    ### 1 - Genes used when using checker texture

    toolbox.register("checker_scale", random.uniform, 0, 50)

    ### 2 - Genes used when using magic texture

    toolbox.register("magic_depth", random.randint, 0, 10)
    toolbox.register("magic_scale", random.uniform, 0, 50)
    toolbox.register("magic_distortion", random.uniform, -10, 10)

    ### 3 - Genes used when using gradient texture
    # 0 - Linear
    # 1 - Quadratic
    # 2 - Easing
    # 3 - Diagonal
    # 4 - Spherical
    # 5 - Quadratic Sphere
    # 6 - Radial
    toolbox.register("gradient_type", random.randint, 0, 6)

    mat_genepool = (toolbox.bsdf_metallic, toolbox.bsdf_specular, toolbox.bsdf_roughness,
                toolbox.color_ramp_1_1_r, toolbox.color_ramp_1_1_g, toolbox.color_ramp_1_1_b,
                toolbox.color_ramp_1_2_r, toolbox.color_ramp_1_2_g, toolbox.color_ramp_1_2_b,
                toolbox.color_ramp_1_1_pos, toolbox.color_ramp_1_2_pos,
                toolbox.color_ramp_2_1_r, toolbox.color_ramp_2_1_g, toolbox.color_ramp_2_1_b,
                toolbox.color_ramp_2_2_r, toolbox.color_ramp_2_2_g, toolbox.color_ramp_2_2_b,
                toolbox.color_ramp_2_1_pos, toolbox.color_ramp_2_2_pos,
                toolbox.texture_mixing_1, toolbox.texture_mixing_2, toolbox.texture_mixing_fac,
                toolbox.base_color_noise_scale, toolbox.base_color_noise_detail,
                toolbox.base_color_noise_roughness, toolbox.base_color_noise_distortion,
                toolbox.checker_scale,
                toolbox.magic_depth, toolbox.magic_scale, toolbox.magic_distortion,
                toolbox.gradient_type)

    form_genepool = (toolbox.faces, toolbox.scale, toolbox.extrude)

    toolbox.register("mat_individual", tools.initCycle, creator.Individual, mat_genepool, n=1)
    toolbox.register("form_individual", tools.initCycle, creator.Individual, form_genepool, n=n_extrusions)

    toolbox.register("mat_population", tools.initRepeat, list, toolbox.mat_individual)
    toolbox.register("form_population", tools.initRepeat, list, toolbox.form_individual)

    # Evolution functions: mate, mutate and select individuals.
    toolbox.register("mate", tools.cxTwoPoint)
    toolbox.register("mutate", tools.mutFlipBit, indpb=0.05)
    toolbox.register("select", tools.selTournament, tournsize=3)

    mat_pop = toolbox.mat_population(n=pop_size*2)
    form_pop = toolbox.form_population(n=pop_size)

    # get
    objs = {}
    mats = {}
    for n in range(pop_size):
        mat_individual = mat_pop[n]
        form_individual = form_pop[n]
        o,m,a = blenderDraw(form_individual, mat_individual, n_extrusions, n, (0, 0, 0))
        objs[n] = o
        mats[n] = m

    for n in range(pop_size, pop_size * 2):
        mats[n] = blenderCreateTex(mat_pop[n], n)
       
    return (objs, mats)

# Select all
bpy.ops.object.select_all(action='SELECT')
bpy.ops.object.delete()
bpy.ops.object.select_all(action='DESELECT')

#WIDTH = 1024
#HEIGHT = 1024
POP = $$POP$$
#SAVE_LOCATION = r'C:\Users\Mathis\Downloads\'
#FNAME = 'test'

WIDTH = $$WIDTH$$
HEIGHT = $$HEIGHT$$
SAVE_LOCATION = "$$SAVE_LOCATION$$"
FNAME = r'$$FNAME$$'
BLENDER_VERSION = $$BLENDER_VERSION$$
random.seed($$SEED$$)


objs,mats = genetic(POP)

bpy.ops.mesh.primitive_plane_add(location=(1000,0,0))
plane = bpy.context.active_object
bpy.ops.object.select_all(action='DESELECT')


# Switch rendering engine to cycles
bpy.context.scene.render.engine = 'CYCLES'
bpy.context.scene.cycles.device = 'GPU'

bpy.context.scene.cycles.bake_type = 'DIFFUSE'
bpy.context.scene.render.bake.use_pass_direct = False
bpy.context.scene.render.bake.use_pass_indirect = False
bpy.context.scene.render.bake.use_pass_color = True

#For all objects, create a UV map
for i in range(POP):
    obj = objs[i]
    mat = mats[i]
        
    # Select both object and material from their names
    bpy.context.view_layer.objects.active = obj
    # bpy.ops.transform.translate(value=(0, i * 10, 0))
    bpy.ops.object.mode_set(mode='EDIT')
    bpy.ops.mesh.select_all(action='SELECT')
    bpy.ops.uv.smart_project()
    bpy.ops.mesh.select_all(action='DESELECT')
    bpy.ops.object.mode_set(mode='OBJECT')
    
    # Create a new image
    image = bpy.data.images.new("BakedMaterial " + str(i), WIDTH, HEIGHT)

    tex_node = mat.node_tree.nodes.new('ShaderNodeTexImage')
    tex_node.name = "Baking Texture " + str(i)
    tex_node.select = True
    mat.node_tree.nodes.active = tex_node
    tex_node.image = image
    tex_node.select = False

    bpy.context.view_layer.objects.active = obj
    #select
    obj.select_set(True)
    
    bpy.ops.object.bake(type='DIFFUSE', save_mode='EXTERNAL')
    image.save_render(SAVE_LOCATION+FNAME+str(i)+'.png')
    
    bsdf = mat.node_tree.nodes.get('Principled BSDF')
    
    # Change tex_node image to the generated one
    tex_node.image = image
    
    # Link the image to the base color
    mat.node_tree.links.new(tex_node.outputs['Color'], bsdf.inputs['Base Color'])
    
    #mapping = mat.node_tree.nodes.new('ShaderNodeMapping')
    #TexCoord = mat.node_tree.nodes.new('ShaderNodeTexCoord')
    
    #mat.node_tree.links.new(TexCoord.outputs['UV'], mapping.inputs['Vector'])
    #mat.node_tree.links.new(mapping.outputs['Vector'], tex_node.inputs['Vector'])
    
    obj.select_set(False)



plane.select_set(True)

for i in range(POP,POP*2):
    mat = mats[i]
    plane.data.materials.append(mat)
    plane.active_material = mat

    image = bpy.data.images.new("BakedMaterialRoom " + str(i-POP), WIDTH, HEIGHT)
    tex_node = mat.node_tree.nodes.new('ShaderNodeTexImage')
    tex_node.name = "Baking Texture Room " + str(i-POP)
    tex_node.select = True
    mat.node_tree.nodes.active = tex_node
    tex_node.image = image
    tex_node.select = False

    bpy.context.view_layer.objects.active = plane
    #select
    plane.select_set(True)

    bpy.ops.object.bake(type='DIFFUSE', save_mode='EXTERNAL')
    image.save_render(SAVE_LOCATION+"room"+str(i-POP)+'.png')


plane.select_set(True)
bpy.ops.object.delete(use_global=False, confirm=False)


if BLENDER_VERSION <= 3:
    bpy.ops.export_scene.fbx(filepath=SAVE_LOCATION+FNAME+'.obj', export_uv=True, export_normals=True, export_materials=True)
else:
    bpy.ops.wm.obj_export(filepath=SAVE_LOCATION+FNAME+'.obj', export_uv=True, export_normals=True, export_materials=True)

        