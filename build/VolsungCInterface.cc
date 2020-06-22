
#include "../Volsung/include/Volsung.hh"
#include <cstring>
#include <unordered_map>

struct Program_With_Parameters {
    Volsung::Program program;
    std::vector<std::string> parameter_names;
    std::vector<float> parameter_values;
    bool stereo;
};

std::unordered_map<int, Program_With_Parameters> programs;
std::string debug_text = "";
int program_index;

extern "C" {

int VLSNG_create_program() {
    programs[program_index] = Program_With_Parameters();
    const int handle = program_index++;
    return handle;
}

void VLSNG_destroy_program(int handle) {
    programs.erase(handle);
}

void VLSNG_run(int handle, float* data) {
    Volsung::MultichannelBuffer buffer = programs[handle].program.run({ Volsung::AudioBuffer::zero });
    std::copy(buffer[0].begin(), buffer[0].end(), data);

    if (programs[handle].stereo)
        std::copy(buffer[1].begin(), buffer[1].end(), data + Volsung::AudioBuffer::blocksize);
}

void VLSNG_interpret_program(int handle, const char* code, bool stereo) {
    programs[handle].stereo = stereo;
    programs[handle].program.configure_io(0, stereo + 1);
    programs[handle].program.reset();

    Volsung::Parser parser;
    parser.source_code = code;

    try {
        for (std::size_t n = 0; n < programs[handle].parameter_names.size(); n++) {
            programs[handle].program.create_user_object(programs[handle].parameter_names[n], 0, 1, programs[handle].parameter_values.data(),
                                      [n] (const Volsung::MultichannelBuffer&, Volsung::MultichannelBuffer& out, std::any values)
            {
                float* data = std::any_cast<float*>(values);
                for (size_t s = 0; s < Volsung::AudioBuffer::blocksize; s++) {
                    out[0][s] = data[n];
                }
            });
        }
    }

    catch (const Volsung::VolsungException& exception) {
        Volsung::log(std::string(exception.what()));
        programs[handle].program.reset();
        return;
    }

    if (parser.parse_program(programs[handle].program)) Volsung::log("Parsed successfully!");
}

void VLSNG_initialize() {
    Volsung::set_debug_callback([] (std::string message) {
        debug_text += message + "\n";
    });
}

void VLSNG_register_parameters(int handle, char const* const* names, int size) {
    programs[handle].parameter_names.clear();
    programs[handle].parameter_names = std::vector<std::string>(names, names + size);
    programs[handle].parameter_values.resize(size);
}

void VLSNG_update_parameters(int handle, const float* values, int size) {
    std::memcpy(programs[handle].parameter_values.data(), values, size * sizeof(float));
}

const char* VLSNG_read_debug_text() {
    return debug_text.c_str();
}

void VLSNG_clear_debug_text() {
    debug_text.clear();
}

}
